using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Campaigns;

namespace QPhising.Application.Features.Campaigns.CreateCampaign;

public sealed record CreateCampaignCommand(
    string Name,
    TemplateType TemplateType,
    string HtmlContent,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate) : IRequest<Result<CreateCampaignResponse>>;

public sealed record CreateCampaignResponse(
    Guid Id,
    string Name,
    TemplateType TemplateType,
    string HtmlContent,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    CampaignStatus Status);
