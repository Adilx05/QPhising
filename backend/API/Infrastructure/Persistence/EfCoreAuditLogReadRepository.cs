using Microsoft.EntityFrameworkCore;
using QPhising.Api.Infrastructure.Persistence.Entities;
using QPhising.Application.Contracts.Abstractions.Audit;
using QPhising.Application.Contracts.Responses.Audit;

namespace QPhising.Api.Infrastructure.Persistence;

public sealed class EfCoreAuditLogReadRepository : IAuditLogReadRepository
{
    private readonly QPhisingDbContext _dbContext;

    public EfCoreAuditLogReadRepository(QPhisingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AuditLogPageResult> QueryAsync(AuditLogFilter filter, CancellationToken cancellationToken)
    {
        var query = _dbContext.AuditLogEntries.AsNoTracking();

        if (filter.FromUtc.HasValue)
        {
            query = query.Where(entry => entry.TimestampUtc >= filter.FromUtc.Value);
        }

        if (filter.ToUtc.HasValue)
        {
            query = query.Where(entry => entry.TimestampUtc <= filter.ToUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Actor))
        {
            var actorTerm = filter.Actor.Trim().ToLowerInvariant();
            query = query.Where(entry => entry.Actor.ToLower().Contains(actorTerm));
        }

        if (!string.IsNullOrWhiteSpace(filter.Endpoint))
        {
            var endpointTerm = filter.Endpoint.Trim().ToLowerInvariant();
            query = query.Where(entry => entry.Resource.ToLower().Contains(endpointTerm));
        }

        if (!string.IsNullOrWhiteSpace(filter.CorrelationId))
        {
            var correlationIdTerm = filter.CorrelationId.Trim().ToLowerInvariant();
            query = query.Where(entry => entry.CorrelationId.ToLower().Contains(correlationIdTerm));
        }

        if (filter.OutcomeCode.HasValue)
        {
            query = query.Where(entry => entry.OutcomeCode == filter.OutcomeCode.Value);
        }

        query = ApplySorting(query, filter.SortBy, filter.SortDirection);

        var totalCount = await query.CountAsync(cancellationToken);
        var skip = (filter.Page - 1) * filter.PageSize;

        var page = await query
            .Skip(skip)
            .Take(filter.PageSize)
            .Select(entry => new AuditLogEntryResult(
                entry.Id,
                entry.TimestampUtc,
                entry.Actor,
                entry.Action,
                entry.Resource,
                entry.Outcome,
                entry.OutcomeCode,
                entry.CorrelationId,
                entry.IpHash))
            .ToArrayAsync(cancellationToken);

        return new AuditLogPageResult(page, filter.Page, filter.PageSize, totalCount);
    }

    private static IQueryable<AuditLogEntryEntity> ApplySorting(
        IQueryable<AuditLogEntryEntity> query,
        AuditLogSortField sortBy,
        SortDirection sortDirection)
    {
        return (sortBy, sortDirection) switch
        {
            (AuditLogSortField.Actor, SortDirection.Asc) => query.OrderBy(entry => entry.Actor).ThenBy(entry => entry.TimestampUtc),
            (AuditLogSortField.Actor, SortDirection.Desc) => query.OrderByDescending(entry => entry.Actor).ThenByDescending(entry => entry.TimestampUtc),
            (AuditLogSortField.Action, SortDirection.Asc) => query.OrderBy(entry => entry.Action).ThenBy(entry => entry.TimestampUtc),
            (AuditLogSortField.Action, SortDirection.Desc) => query.OrderByDescending(entry => entry.Action).ThenByDescending(entry => entry.TimestampUtc),
            (AuditLogSortField.Resource, SortDirection.Asc) => query.OrderBy(entry => entry.Resource).ThenBy(entry => entry.TimestampUtc),
            (AuditLogSortField.Resource, SortDirection.Desc) => query.OrderByDescending(entry => entry.Resource).ThenByDescending(entry => entry.TimestampUtc),
            (AuditLogSortField.OutcomeCode, SortDirection.Asc) => query.OrderBy(entry => entry.OutcomeCode).ThenBy(entry => entry.TimestampUtc),
            (AuditLogSortField.OutcomeCode, SortDirection.Desc) => query.OrderByDescending(entry => entry.OutcomeCode).ThenByDescending(entry => entry.TimestampUtc),
            (AuditLogSortField.Timestamp, SortDirection.Asc) => query.OrderBy(entry => entry.TimestampUtc),
            _ => query.OrderByDescending(entry => entry.TimestampUtc)
        };
    }
}
