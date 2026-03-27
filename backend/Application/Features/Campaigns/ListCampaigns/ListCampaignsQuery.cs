using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Campaigns;

namespace QPhising.Application.Features.Campaigns.ListCampaigns;

public sealed record ListCampaignsQuery(
    IReadOnlyCollection<CampaignStatus>? Statuses = null,
    IReadOnlyCollection<TemplateType>? TemplateTypes = null,
    DateTimeOffset? StartsOnOrAfter = null,
    DateTimeOffset? EndsOnOrBefore = null,
    int Skip = 0,
    int Take = 50) : IRequest<Result<ListCampaignsResponse>>;

public sealed record ListCampaignsResponse(
    int Skip,
    int Take,
    IReadOnlyCollection<CampaignListItemResponse> Items);

public sealed record CampaignListItemResponse(
    Guid Id,
    string Name,
    TemplateType TemplateType,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    CampaignStatus Status);
