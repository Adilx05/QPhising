using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Campaigns;

namespace QPhising.Application.Features.Campaigns.ActivateCampaign;

public sealed record ActivateCampaignCommand(Guid CampaignId) : IRequest<Result<ActivateCampaignResponse>>;

public sealed record ActivateCampaignResponse(
    Guid Id,
    CampaignStatus Status,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate);
