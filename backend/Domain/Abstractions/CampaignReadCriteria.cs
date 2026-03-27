using QPhising.Domain.Campaigns;

namespace QPhising.Domain.Abstractions;

public sealed record CampaignReadCriteria(
    IReadOnlyCollection<CampaignStatus>? Statuses = null,
    IReadOnlyCollection<TemplateType>? TemplateTypes = null,
    DateTimeOffset? StartsOnOrAfter = null,
    DateTimeOffset? EndsOnOrBefore = null,
    int? Skip = null,
    int? Take = null);
