using QPhising.Domain.Abstractions;
using QPhising.Domain.Tracking;
using Microsoft.EntityFrameworkCore;

namespace QPhising.Infrastructure.Persistence.Repositories;

public sealed class TrackingClickRepository(QPhisingDbContext dbContext) : ITrackingClickRepository
{
    public async Task AddAsync(TrackingClick click, CancellationToken cancellationToken = default)
    {
        await dbContext.TrackingClicks.AddAsync(click, cancellationToken);
    }

    public async Task<int> DeleteOlderThanAsync(DateTimeOffset cutoffUtc, int batchSize, CancellationToken cancellationToken = default)
    {
        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be greater than zero.");
        }

        List<Guid> staleIds = await dbContext.TrackingClicks
            .Where(click => click.ClickedAtUtc < cutoffUtc)
            .OrderBy(click => click.ClickedAtUtc)
            .Select(click => click.Id)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        if (staleIds.Count == 0)
        {
            return 0;
        }

        return await dbContext.TrackingClicks
            .Where(click => staleIds.Contains(click.Id))
            .ExecuteDeleteAsync(cancellationToken);
    }
}
