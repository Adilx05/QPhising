using QPhising.Domain.Tracking.Entities;

namespace QPhising.Application.Contracts.Abstractions.Tracking;

public interface IVisitEventRepository
{
    Task<bool> ExistsDuplicateAsync(
        Guid trackingPageId,
        string sessionId,
        string visitorFingerprint,
        DateTimeOffset occurredAtUtc,
        TimeSpan deduplicationWindow,
        CancellationToken cancellationToken);

    Task SaveAsync(VisitEventEntity visitEvent, CancellationToken cancellationToken);

    Task<int> CountTotalAsync(Guid trackingPageId, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, CancellationToken cancellationToken);

    Task<int> CountUniqueVisitorsAsync(Guid trackingPageId, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, CancellationToken cancellationToken);

    Task<DateTimeOffset?> GetLastVisitAtUtcAsync(Guid trackingPageId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TrackingVisitTrendBucket>> GetTrendBucketsAsync(
        Guid trackingPageId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        int bucketSizeMinutes,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<VisitEventEntity>> ListRecentAsync(
        Guid trackingPageId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        int limit,
        CancellationToken cancellationToken);

    Task<int> CountTotalAcrossPagesAsync(
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        bool excludeBots,
        CancellationToken cancellationToken);

    Task<int> CountUniqueVisitorsAcrossPagesAsync(
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        bool excludeBots,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TrackingTopPageMetric>> ListTopPagesAsync(
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        bool excludeBots,
        int limit,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TrackingRecentVisitMetric>> ListRecentAcrossPagesAsync(
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        bool excludeBots,
        int limit,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TrackingVisitTrendBucket>> GetTrendBucketsAcrossPagesAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        TrackingVisitTrendBucketWindow window,
        int timezoneOffsetMinutes,
        bool excludeBots,
        CancellationToken cancellationToken);
}
