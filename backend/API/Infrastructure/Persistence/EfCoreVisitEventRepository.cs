using Microsoft.EntityFrameworkCore;
using QPhising.Api.Infrastructure.Persistence.Mapping;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Domain.Tracking.Entities;
using PersistenceVisitEventEntity = QPhising.Api.Infrastructure.Persistence.Entities.VisitEventEntity;

namespace QPhising.Api.Infrastructure.Persistence;

public sealed class EfCoreVisitEventRepository : IVisitEventRepository
{
    private readonly QPhisingDbContext _dbContext;

    public EfCoreVisitEventRepository(QPhisingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsDuplicateAsync(
        Guid trackingPageId,
        string sessionId,
        string visitorFingerprint,
        DateTimeOffset occurredAtUtc,
        TimeSpan deduplicationWindow,
        CancellationToken cancellationToken)
    {
        var windowStartUtc = occurredAtUtc.Subtract(deduplicationWindow);
        var windowEndUtc = occurredAtUtc.Add(deduplicationWindow);
        var normalizedSessionId = sessionId.Trim();
        var normalizedFingerprint = visitorFingerprint.Trim();

        return _dbContext.VisitEvents.AnyAsync(
            visit => visit.TrackingPageId == trackingPageId
                     && visit.SessionId == normalizedSessionId
                     && visit.VisitorFingerprint == normalizedFingerprint
                     && visit.OccurredAtUtc >= windowStartUtc
                     && visit.OccurredAtUtc <= windowEndUtc,
            cancellationToken);
    }

    public Task SaveAsync(VisitEventEntity visitEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(visitEvent);

        _dbContext.VisitEvents.Add(visitEvent.ToPersistenceEntity());
        return Task.CompletedTask;
    }

    public Task<int> CountTotalAsync(Guid trackingPageId, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, CancellationToken cancellationToken)
    {
        return ApplyRangeFilter(_dbContext.VisitEvents.AsNoTracking(), trackingPageId, fromUtc, toUtc).CountAsync(cancellationToken);
    }

    public Task<int> CountUniqueVisitorsAsync(Guid trackingPageId, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, CancellationToken cancellationToken)
    {
        return ApplyRangeFilter(_dbContext.VisitEvents.AsNoTracking(), trackingPageId, fromUtc, toUtc)
            .Select(visit => visit.VisitorFingerprint)
            .Distinct()
            .CountAsync(cancellationToken);
    }

    public Task<DateTimeOffset?> GetLastVisitAtUtcAsync(Guid trackingPageId, CancellationToken cancellationToken)
    {
        return _dbContext.VisitEvents
            .AsNoTracking()
            .Where(visit => visit.TrackingPageId == trackingPageId)
            .OrderByDescending(visit => visit.OccurredAtUtc)
            .Select(visit => (DateTimeOffset?)visit.OccurredAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TrackingVisitTrendBucket>> GetTrendBucketsAsync(
        Guid trackingPageId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        int bucketSizeMinutes,
        CancellationToken cancellationToken)
    {
        var visits = await _dbContext.VisitEvents
            .AsNoTracking()
            .Where(visit => visit.TrackingPageId == trackingPageId && visit.OccurredAtUtc >= fromUtc && visit.OccurredAtUtc <= toUtc)
            .Select(visit => new { visit.OccurredAtUtc, visit.VisitorFingerprint })
            .ToListAsync(cancellationToken);

        var buckets = visits
            .GroupBy(visit => FloorBucket(visit.OccurredAtUtc, bucketSizeMinutes))
            .OrderBy(group => group.Key)
            .Select(group => new TrackingVisitTrendBucket(
                BucketStartUtc: group.Key,
                TotalVisits: group.Count(),
                UniqueVisitors: group.Select(item => item.VisitorFingerprint).Distinct(StringComparer.Ordinal).Count()))
            .ToArray();

        return buckets;
    }

    public async Task<IReadOnlyCollection<VisitEventEntity>> ListRecentAsync(
        Guid trackingPageId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        int limit,
        CancellationToken cancellationToken)
    {
        var entities = await ApplyRangeFilter(_dbContext.VisitEvents.AsNoTracking(), trackingPageId, fromUtc, toUtc)
            .OrderByDescending(visit => visit.OccurredAtUtc)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return entities.Select(entity => entity.ToDomainEntity()).ToArray();
    }

    private static IQueryable<PersistenceVisitEventEntity> ApplyRangeFilter(
        IQueryable<PersistenceVisitEventEntity> source,
        Guid trackingPageId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc)
    {
        var query = source.Where(visit => visit.TrackingPageId == trackingPageId);

        if (fromUtc.HasValue)
        {
            query = query.Where(visit => visit.OccurredAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(visit => visit.OccurredAtUtc <= toUtc.Value);
        }

        return query;
    }

    private static DateTimeOffset FloorBucket(DateTimeOffset value, int bucketSizeMinutes)
    {
        var utcValue = value.ToUniversalTime();
        var bucketTicks = TimeSpan.FromMinutes(bucketSizeMinutes).Ticks;
        var flooredTicks = utcValue.Ticks - (utcValue.Ticks % bucketTicks);
        return new DateTimeOffset(flooredTicks, TimeSpan.Zero);
    }
}
