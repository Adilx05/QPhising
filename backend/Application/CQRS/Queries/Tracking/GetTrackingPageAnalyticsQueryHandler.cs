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

        var fromUtc = request.FromUtc ?? DateTimeOffset.UtcNow.AddDays(-7);
        var toUtc = request.ToUtc ?? DateTimeOffset.UtcNow;

        var totalVisitsTask = _visitEventRepository.CountTotalAsync(trackingPage.Id, request.FromUtc, request.ToUtc, cancellationToken);
        var uniqueVisitorsTask = _visitEventRepository.CountUniqueVisitorsAsync(trackingPage.Id, request.FromUtc, request.ToUtc, cancellationToken);
        var lastVisitAtUtcTask = _visitEventRepository.GetLastVisitAtUtcAsync(trackingPage.Id, cancellationToken);
        var trendBucketsTask = _visitEventRepository.GetTrendBucketsAsync(
            trackingPage.Id,
            fromUtc,
            toUtc,
            request.TrendBucketSizeMinutes,
            cancellationToken);
        var recentVisitsTask = _visitEventRepository.ListRecentAsync(
            trackingPage.Id,
            request.FromUtc,
            request.ToUtc,
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
