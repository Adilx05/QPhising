namespace QPhising.Api.Contracts.Campaign;

public sealed record CreateCampaignRequest(
    string Name,
    string TrackingPageSlug,
    string TrackingPageTitle,
    string DestinationUrl,
    string? TrackingPageDescription,
    Guid? TemplateId);
