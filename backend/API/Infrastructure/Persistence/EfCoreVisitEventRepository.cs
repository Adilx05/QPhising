using Microsoft.EntityFrameworkCore;
using QPhising.Api.Infrastructure.Persistence.Mapping;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Domain.Tracking.Enums;
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

    public Task<int> CountTotalAcrossPagesAsync(
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        bool excludeBots,
        CancellationToken cancellationToken)
    {
        return ApplyRangeFilterAcrossPages(_dbContext.VisitEvents.AsNoTracking(), fromUtc, toUtc, excludeBots)
            .CountAsync(cancellationToken);
    }

    public Task<int> CountUniqueVisitorsAcrossPagesAsync(
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        bool excludeBots,
        CancellationToken cancellationToken)
    {
        return ApplyRangeFilterAcrossPages(_dbContext.VisitEvents.AsNoTracking(), fromUtc, toUtc, excludeBots)
            .Select(visit => string.IsNullOrWhiteSpace(visit.SessionId) ? visit.VisitorFingerprint : visit.SessionId)
            .Distinct()
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TrackingTopPageMetric>> ListTopPagesAsync(
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        bool excludeBots,
        int limit,
        CancellationToken cancellationToken)
    {
        var query = ApplyRangeFilterAcrossPages(_dbContext.VisitEvents.AsNoTracking(), fromUtc, toUtc, excludeBots);

        var totalByPage = await query
            .GroupBy(visit => new { visit.TrackingPageId, visit.TrackingPage.Slug, visit.TrackingPage.Title })
            .Select(group => new
            {
                group.Key.TrackingPageId,
                group.Key.Slug,
                group.Key.Title,
                TotalVisits = group.Count()
            })
            .ToListAsync(cancellationToken);

        var uniqueByPage = await query
            .Select(visit => new
            {
                visit.TrackingPageId,
                UniqueKey = string.IsNullOrWhiteSpace(visit.SessionId) ? visit.VisitorFingerprint : visit.SessionId
            })
            .Distinct()
            .GroupBy(visit => visit.TrackingPageId)
            .Select(group => new
            {
                TrackingPageId = group.Key,
                UniqueVisitors = group.Count()
            })
            .ToDictionaryAsync(entry => entry.TrackingPageId, entry => entry.UniqueVisitors, cancellationToken);

        return totalByPage
            .Select(metric => new TrackingTopPageMetric(
                TrackingPageId: metric.TrackingPageId,
                Slug: metric.Slug,
                Title: metric.Title,
                TotalVisits: metric.TotalVisits,
                UniqueVisitors: uniqueByPage.GetValueOrDefault(metric.TrackingPageId)))
            .OrderByDescending(metric => metric.TotalVisits)
            .ThenByDescending(metric => metric.UniqueVisitors)
            .ThenBy(metric => metric.Slug, StringComparer.Ordinal)
            .Take(limit)
            .ToArray();
    }

    public async Task<IReadOnlyCollection<TrackingRecentVisitMetric>> ListRecentAcrossPagesAsync(
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        bool excludeBots,
        int limit,
        CancellationToken cancellationToken)
    {
        var visits = await ApplyRangeFilterAcrossPages(_dbContext.VisitEvents.AsNoTracking(), fromUtc, toUtc, excludeBots)
            .OrderByDescending(visit => visit.OccurredAtUtc)
            .ThenByDescending(visit => visit.Id)
            .Take(limit)
            .Select(visit => new
            {
                visit.Id,
                visit.TrackingPageId,
                visit.TrackingPage.Slug,
                visit.OccurredAtUtc,
                visit.SessionId,
                visit.VisitorFingerprint,
                visit.UserAgent,
                visit.ReferrerUrl,
                visit.IpHash,
                visit.IpAddressHashPolicy
            })
            .ToListAsync(cancellationToken);

        return visits
            .Select(visit => new TrackingRecentVisitMetric(
                VisitId: visit.Id,
                TrackingPageId: visit.TrackingPageId,
                TrackingPageSlug: visit.Slug,
                OccurredAtUtc: visit.OccurredAtUtc,
                SessionId: visit.SessionId,
                VisitorFingerprint: visit.VisitorFingerprint,
                UserAgent: visit.UserAgent,
                ReferrerUrl: visit.ReferrerUrl,
                IpHash: visit.IpHash,
                IpAddressHashPolicy: (IpAddressHashPolicy)visit.IpAddressHashPolicy))
            .ToArray();
    }

    public async Task<IReadOnlyCollection<TrackingVisitTrendBucket>> GetTrendBucketsAcrossPagesAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        TrackingVisitTrendBucketWindow window,
        int timezoneOffsetMinutes,
        bool excludeBots,
        CancellationToken cancellationToken)
    {
        var visits = await ApplyRangeFilterAcrossPages(_dbContext.VisitEvents.AsNoTracking(), fromUtc, toUtc, excludeBots)
            .Select(visit => new { visit.OccurredAtUtc, visit.SessionId, visit.VisitorFingerprint })
            .ToListAsync(cancellationToken);

        return visits
            .GroupBy(visit => AlignTrendBucket(visit.OccurredAtUtc, window, timezoneOffsetMinutes))
            .OrderBy(group => group.Key)
            .Select(group => new TrackingVisitTrendBucket(
                BucketStartUtc: group.Key,
                TotalVisits: group.Count(),
                UniqueVisitors: group.Select(visit => string.IsNullOrWhiteSpace(visit.SessionId) ? visit.VisitorFingerprint : visit.SessionId)
                    .Distinct(StringComparer.Ordinal)
                    .Count()))
            .ToArray();
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

    private static IQueryable<PersistenceVisitEventEntity> ApplyRangeFilterAcrossPages(
        IQueryable<PersistenceVisitEventEntity> source,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        bool excludeBots)
    {
        var query = source;

        if (fromUtc.HasValue)
        {
            query = query.Where(visit => visit.OccurredAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(visit => visit.OccurredAtUtc <= toUtc.Value);
        }

        if (excludeBots)
        {
            query = query.Where(visit => visit.TrackingPage.EnableBotFiltering != true || !IsBotUserAgent(visit.UserAgent));
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

    private static DateTimeOffset AlignTrendBucket(
        DateTimeOffset value,
        TrackingVisitTrendBucketWindow window,
        int timezoneOffsetMinutes)
    {
        var offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);
        var local = value.ToOffset(offset);

        var localStart = window switch
        {
            TrackingVisitTrendBucketWindow.Hour => new DateTimeOffset(local.Year, local.Month, local.Day, local.Hour, 0, 0, offset),
            TrackingVisitTrendBucketWindow.Day => new DateTimeOffset(local.Year, local.Month, local.Day, 0, 0, 0, offset),
            TrackingVisitTrendBucketWindow.Week => AlignToWeekStart(local, offset),
            _ => throw new ArgumentOutOfRangeException(nameof(window), window, "Unsupported trend window.")
        };

        return localStart.ToUniversalTime();
    }

    private static DateTimeOffset AlignToWeekStart(DateTimeOffset local, TimeSpan offset)
    {
        var startOfDay = new DateTimeOffset(local.Year, local.Month, local.Day, 0, 0, 0, offset);
        var daysSinceMonday = ((int)startOfDay.DayOfWeek + 6) % 7;
        return startOfDay.AddDays(-daysSinceMonday);
    }

    private static bool IsBotUserAgent(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return false;
        }

        var normalized = userAgent.Trim();
        return normalized.Contains("bot", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("spider", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("crawler", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("slurp", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("headless", StringComparison.OrdinalIgnoreCase);
    }
}
