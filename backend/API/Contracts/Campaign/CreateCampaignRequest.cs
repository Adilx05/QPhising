namespace QPhising.Api.Contracts.Campaign;

public sealed record CreateCampaignRequest(
    string Name,
    string TrackingPageSlug,
    string TrackingPageTitle,
    string? TrackingPageDescription,
    Guid? TemplateId,
    string? HtmlContent,
    DateTimeOffset? ValidFromUtc,
    DateTimeOffset? ValidUntilUtc);
