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

        var totalVisits = await _visitEventRepository.CountTotalAsync(trackingPage.Id, effectiveFromUtc, effectiveToUtc, cancellationToken);
        var uniqueVisitors = await _visitEventRepository.CountUniqueVisitorsAsync(trackingPage.Id, effectiveFromUtc, effectiveToUtc, cancellationToken);
        var lastVisitAtUtc = await _visitEventRepository.GetLastVisitAtUtcAsync(trackingPage.Id, cancellationToken);
        var trendBuckets = await _visitEventRepository.GetTrendBucketsAsync(
            trackingPage.Id,
            effectiveFromUtc,
            effectiveToUtc,
            request.TrendBucketSizeMinutes,
            cancellationToken);
        var recentVisits = await _visitEventRepository.ListRecentAsync(
            trackingPage.Id,
            effectiveFromUtc,
            effectiveToUtc,
            request.RecentVisitLimit,
            cancellationToken);

        return new TrackingPageAnalyticsResult(
            TrackingPageId: trackingPage.Id,
            Slug: trackingPage.Slug.Value,
            Summary: new TrackingAnalyticsSummaryResult(totalVisits, uniqueVisitors, lastVisitAtUtc),
            Trends: trendBuckets.Select(bucket => bucket.ToResult()).ToArray(),
            RecentVisits: recentVisits.Select(visit => visit.ToResult()).ToArray());
    }
}
