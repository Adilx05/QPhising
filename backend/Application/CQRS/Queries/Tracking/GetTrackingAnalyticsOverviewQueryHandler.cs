using MediatR;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Application.Contracts.Responses.Tracking;
using QPhising.Application.Mapping.Tracking;

namespace QPhising.Application.CQRS.Queries.Tracking;

public sealed class GetTrackingAnalyticsOverviewQueryHandler : IRequestHandler<GetTrackingAnalyticsOverviewQuery, TrackingAnalyticsOverviewResult>
{
    private static readonly IReadOnlyCollection<TrackingMetricDefinitionResult> MetricDefinitions =
    [
        new(
            Metric: "totalVisits",
            Definition: "Counts visit events inside the requested UTC range after optional bot exclusion.",
            EdgeCaseBehavior: "Duplicate hits are included unless deduplicated at ingestion time; timezone conversion does not change total count."),
        new(
            Metric: "uniqueVisitors",
            Definition: "Counts distinct visitor keys using sessionId when present; falls back to visitorFingerprint.",
            EdgeCaseBehavior: "If both identifiers are blank the event uses fingerprint key and remains countable; collisions may undercount."),
        new(
            Metric: "topPages",
            Definition: "Ranks tracking pages by total visits and then unique visitors for tie-breaking.",
            EdgeCaseBehavior: "Pages with zero visits are excluded; when totals tie, stable ordering falls back to slug."),
        new(
            Metric: "trendBuckets",
            Definition: "Aggregates visits into hour/day/week buckets aligned to the provided timezone offset.",
            EdgeCaseBehavior: "Bucket boundaries are computed in local-offset time and converted back to UTC; DST is represented by offset input."),
        new(
            Metric: "recentVisits",
            Definition: "Returns the newest visits in descending occurrence order across all pages.",
            EdgeCaseBehavior: "When timestamps tie, deterministic ordering falls back to visit identifier.")
    ];

    private readonly IVisitEventRepository _visitEventRepository;

    public GetTrackingAnalyticsOverviewQueryHandler(IVisitEventRepository visitEventRepository)
    {
        _visitEventRepository = visitEventRepository;
    }

    public async Task<TrackingAnalyticsOverviewResult> Handle(GetTrackingAnalyticsOverviewQuery request, CancellationToken cancellationToken)
    {
        var fromUtc = request.FromUtc ?? DateTimeOffset.UtcNow.AddDays(-7);
        var toUtc = request.ToUtc ?? DateTimeOffset.UtcNow;

        var totalVisitsTask = _visitEventRepository.CountTotalAcrossPagesAsync(fromUtc, toUtc, request.ExcludeBots, cancellationToken);
        var uniqueVisitorsTask = _visitEventRepository.CountUniqueVisitorsAcrossPagesAsync(fromUtc, toUtc, request.ExcludeBots, cancellationToken);
        var topPagesTask = _visitEventRepository.ListTopPagesAsync(fromUtc, toUtc, request.ExcludeBots, request.TopPagesLimit, cancellationToken);
        var recentVisitsTask = _visitEventRepository.ListRecentAcrossPagesAsync(fromUtc, toUtc, request.ExcludeBots, request.RecentVisitLimit, cancellationToken);
        var trendsTask = _visitEventRepository.GetTrendBucketsAcrossPagesAsync(fromUtc, toUtc, request.TrendWindow, request.TimezoneOffsetMinutes, request.ExcludeBots, cancellationToken);

        await Task.WhenAll(totalVisitsTask, uniqueVisitorsTask, topPagesTask, recentVisitsTask, trendsTask);

        var recentVisits = recentVisitsTask.Result;
        var lastVisit = recentVisits.FirstOrDefault();

        var summary = new TrackingAnalyticsSummaryResult(
            TotalVisits: totalVisitsTask.Result,
            UniqueVisitors: uniqueVisitorsTask.Result,
            LastVisitAtUtc: lastVisit?.OccurredAtUtc);

        return new TrackingAnalyticsOverviewResult(
            Summary: summary,
            TopPages: topPagesTask.Result.Select(page => page.ToResult()).ToArray(),
            RecentVisits: recentVisits.Select(visit => visit.ToStreamResult()).ToArray(),
            Trends: trendsTask.Result.Select(trend => trend.ToResult()).ToArray(),
            MetricDefinitions: MetricDefinitions);
    }
}
