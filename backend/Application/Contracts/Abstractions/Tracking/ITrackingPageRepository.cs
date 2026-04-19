using QPhising.Domain.Tracking.Aggregates;

namespace QPhising.Application.Contracts.Abstractions.Tracking;

public interface ITrackingPageRepository
{
    Task<TrackingPageAggregate?> GetByIdAsync(Guid trackingPageId, CancellationToken cancellationToken);

    Task<TrackingPageAggregate?> GetBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TrackingPageAggregate>> ListAsync(CancellationToken cancellationToken);

    Task<bool> SlugExistsAsync(string slug, Guid? excludingTrackingPageId, CancellationToken cancellationToken);

    Task SaveAsync(TrackingPageAggregate aggregate, CancellationToken cancellationToken);

    Task DeleteAsync(TrackingPageAggregate aggregate, CancellationToken cancellationToken);
}
