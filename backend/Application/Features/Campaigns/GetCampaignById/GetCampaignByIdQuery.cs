using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Campaigns;

namespace QPhising.Application.Features.Campaigns.GetCampaignById;

public sealed record GetCampaignByIdQuery(Guid CampaignId) : IRequest<Result<CampaignDetailResponse>>;

public sealed record CampaignDetailResponse(
    Guid Id,
    string Name,
    TemplateType TemplateType,
    string HtmlContent,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    CampaignStatus Status);
