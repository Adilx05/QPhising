using QPhising.Application.Common;
using QPhising.Domain.Campaigns;

namespace QPhising.Application.Common.Abstractions;

public interface ICampaignInteractionGuard
{
    Task<Result<Campaign>> EnsureTrackingInteractionAllowedAsync(Guid campaignId, CancellationToken cancellationToken = default);
}
