using QPhising.Domain.Abstractions;
using QPhising.Domain.Tracking;

namespace QPhising.Infrastructure.Persistence.Repositories;

public sealed class TrackingClickRepository(QPhisingDbContext dbContext) : ITrackingClickRepository
{
    public async Task AddAsync(TrackingClick click, CancellationToken cancellationToken = default)
    {
        await dbContext.TrackingClicks.AddAsync(click, cancellationToken);
    }
}
