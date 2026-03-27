using QPhising.Domain.Campaigns;

namespace QPhising.Domain.Abstractions;

public interface ICampaignRepository
{
    Task<Campaign?> GetByIdAsync(Guid campaignId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Campaign>> ListAsync(
        CampaignReadCriteria criteria,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Campaign>> ListOverlappingWindowAsync(
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd,
        CancellationToken cancellationToken = default);

    Task AddAsync(Campaign campaign, CancellationToken cancellationToken = default);

    void Update(Campaign campaign);
}
