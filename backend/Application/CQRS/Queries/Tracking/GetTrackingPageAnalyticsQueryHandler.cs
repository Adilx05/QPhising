using MediatR;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Application.Contracts.Responses.Tracking;
using QPhising.Application.Mapping.Tracking;

namespace QPhising.Application.CQRS.Queries.Tracking;

public sealed class GetTrackingPageAnalyticsQueryHandler : IRequestHandler<GetTrackingPageAnalyticsQuery, TrackingPageAnalyticsResult>
{
    private readonly ITrackingPageRepository _trackingPageRepository;
    private readonly IVisitEventRepository _visitEventRepository;

    public GetTrackingPageAnalyticsQueryHandler(ITrackingPageRepository trackingPageRepository, IVisitEventRepository visitEventRepository)
    {
        _trackingPageRepository = trackingPageRepository;
        _visitEventRepository = visitEventRepository;
    }

    public async Task<TrackingPageAnalyticsResult> Handle(GetTrackingPageAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var trackingPage = await _trackingPageRepository.GetByIdAsync(request.TrackingPageId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tracking page '{request.TrackingPageId}' was not found.");

        var nowUtc = DateTimeOffset.UtcNow;
        var requestedFromUtc = request.FromUtc ?? nowUtc.AddDays(-7);
        var requestedToUtc = request.ToUtc ?? nowUtc;
        var retentionBoundUtc = trackingPage.Settings is null
            ? (DateTimeOffset?)null
            : nowUtc.AddDays(-trackingPage.Settings.RetentionDays);

        var effectiveFromUtc = retentionBoundUtc.HasValue && requestedFromUtc < retentionBoundUtc.Value
            ? retentionBoundUtc.Value
            : requestedFromUtc;
        var effectiveToUtc = requestedToUtc;

        var totalVisitsTask = _visitEventRepository.CountTotalAsync(trackingPage.Id, effectiveFromUtc, effectiveToUtc, cancellationToken);
        var uniqueVisitorsTask = _visitEventRepository.CountUniqueVisitorsAsync(trackingPage.Id, effectiveFromUtc, effectiveToUtc, cancellationToken);
        var lastVisitAtUtcTask = _visitEventRepository.GetLastVisitAtUtcAsync(trackingPage.Id, cancellationToken);
        var trendBucketsTask = _visitEventRepository.GetTrendBucketsAsync(
            trackingPage.Id,
            effectiveFromUtc,
            effectiveToUtc,
            request.TrendBucketSizeMinutes,
            cancellationToken);
        var recentVisitsTask = _visitEventRepository.ListRecentAsync(
            trackingPage.Id,
            effectiveFromUtc,
            effectiveToUtc,
            request.RecentVisitLimit,
            cancellationToken);

        await Task.WhenAll(totalVisitsTask, uniqueVisitorsTask, lastVisitAtUtcTask, trendBucketsTask, recentVisitsTask);

        return new TrackingPageAnalyticsResult(
            TrackingPageId: trackingPage.Id,
            Slug: trackingPage.Slug.Value,
            Summary: new TrackingAnalyticsSummaryResult(totalVisitsTask.Result, uniqueVisitorsTask.Result, lastVisitAtUtcTask.Result),
            Trends: trendBucketsTask.Result.Select(bucket => bucket.ToResult()).ToArray(),
            RecentVisits: recentVisitsTask.Result.Select(visit => visit.ToResult()).ToArray());
    }
}
