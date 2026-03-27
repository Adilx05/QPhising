using QPhising.Application.Common.Abstractions;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Campaigns;

namespace QPhising.Application.Common;

public sealed class CampaignInteractionGuard(ICampaignRepository campaignRepository) : ICampaignInteractionGuard
{
    public async Task<Result<Campaign>> EnsureTrackingInteractionAllowedAsync(Guid campaignId, CancellationToken cancellationToken = default)
    {
        Campaign? campaign = await campaignRepository.GetByIdAsync(campaignId, cancellationToken);
        if (campaign is null)
        {
            return Result<Campaign>.Failure($"Campaign '{campaignId}' was not found.");
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;
        if (campaign.IsExpired(now))
        {
            return Result<Campaign>.Failure(
                $"Campaign '{campaignId}' expired at '{campaign.EndDate:O}'. Tracking interactions are blocked.");
        }

        return Result<Campaign>.Success(campaign);
    }
}
