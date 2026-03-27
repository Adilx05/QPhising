using Microsoft.EntityFrameworkCore;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Exports;

namespace QPhising.Infrastructure.Persistence.Repositories;

public sealed class ExportJobRepository(QPhisingDbContext dbContext) : IExportJobRepository
{
    public async Task<ExportJob?> GetByIdAsync(Guid exportJobId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ExportJobs.SingleOrDefaultAsync(job => job.Id == exportJobId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ExportJob>> ListAsync(
        ExportJobReadCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        IQueryable<ExportJob> query = dbContext.ExportJobs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(criteria.OwnerUserId))
        {
            string ownerUserId = criteria.OwnerUserId.Trim();
            query = query.Where(job => job.OwnerUserId == ownerUserId);
        }

        if (criteria.Statuses is { Count: > 0 })
        {
            query = query.Where(job => criteria.Statuses.Contains(job.Status));
        }

        if (criteria.ExportTypes is { Count: > 0 })
        {
            query = query.Where(job => criteria.ExportTypes.Contains(job.ExportType));
        }

        if (criteria.Formats is { Count: > 0 })
        {
            query = query.Where(job => criteria.Formats.Contains(job.Format));
        }

        if (criteria.RequestedFrom.HasValue)
        {
            query = query.Where(job => job.RequestedAt >= criteria.RequestedFrom.Value);
        }

        if (criteria.RequestedTo.HasValue)
        {
            query = query.Where(job => job.RequestedAt <= criteria.RequestedTo.Value);
        }

        query = query.OrderByDescending(job => job.RequestedAt).ThenByDescending(job => job.Id);

        if (criteria.Skip is > 0)
        {
            query = query.Skip(criteria.Skip.Value);
        }

        if (criteria.Take is > 0)
        {
            query = query.Take(criteria.Take.Value);
        }

        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task AddAsync(ExportJob exportJob, CancellationToken cancellationToken = default)
    {
        await dbContext.ExportJobs.AddAsync(exportJob, cancellationToken);
    }

    public void Update(ExportJob exportJob)
    {
        dbContext.ExportJobs.Update(exportJob);
    }

    public async Task<IReadOnlyCollection<ExportJob>> ListExpiredWithStoredFileAsync(
        DateTimeOffset asOfUtc,
        int take,
        CancellationToken cancellationToken = default)
    {
        int boundedTake = Math.Max(1, take);

        return await dbContext.ExportJobs
            .Where(job =>
                job.Status == ExportJobStatus.Completed &&
                job.ExpiresAt.HasValue &&
                job.ExpiresAt.Value <= asOfUtc &&
                job.StoragePath != null)
            .OrderBy(job => job.ExpiresAt)
            .ThenBy(job => job.Id)
            .Take(boundedTake)
            .ToArrayAsync(cancellationToken);
    }
}
