using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Campaigns;

namespace QPhising.Application.Features.Campaigns.UpdateCampaign;

public sealed record UpdateCampaignCommand(
    Guid CampaignId,
    string Name,
    TemplateType TemplateType,
    string HtmlContent,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate) : IRequest<Result<UpdateCampaignResponse>>;

public sealed record UpdateCampaignResponse(
    Guid Id,
    string Name,
    TemplateType TemplateType,
    string HtmlContent,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    CampaignStatus Status);
