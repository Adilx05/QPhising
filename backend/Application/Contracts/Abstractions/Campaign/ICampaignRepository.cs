using QPhising.Domain.Campaign.Aggregates;

namespace QPhising.Application.Contracts.Abstractions.Campaign;

public interface ICampaignRepository
{
    Task<CampaignAggregate?> GetByIdAsync(Guid campaignId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CampaignAggregate>> ListAsync(CancellationToken cancellationToken);

    Task SaveAsync(CampaignAggregate aggregate, CancellationToken cancellationToken);

    Task DeleteAsync(CampaignAggregate aggregate, CancellationToken cancellationToken);
}
