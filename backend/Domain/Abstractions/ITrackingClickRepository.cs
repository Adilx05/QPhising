using QPhising.Domain.Tracking;

namespace QPhising.Domain.Abstractions;

public interface ITrackingClickRepository
{
    Task AddAsync(TrackingClick click, CancellationToken cancellationToken = default);

    Task<int> DeleteOlderThanAsync(DateTimeOffset cutoffUtc, int batchSize, CancellationToken cancellationToken = default);
}
