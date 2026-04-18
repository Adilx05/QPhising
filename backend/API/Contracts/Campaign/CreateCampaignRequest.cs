namespace QPhising.Api.Contracts.Campaign;

public sealed record CreateCampaignRequest(
    string Name,
    Guid TemplateId);
