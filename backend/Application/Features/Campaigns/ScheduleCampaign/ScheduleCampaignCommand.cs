using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Campaigns;

namespace QPhising.Application.Features.Campaigns.ScheduleCampaign;

public sealed record ScheduleCampaignCommand(Guid CampaignId) : IRequest<Result<ScheduleCampaignResponse>>;

public sealed record ScheduleCampaignResponse(
    Guid Id,
    CampaignStatus Status,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate);
