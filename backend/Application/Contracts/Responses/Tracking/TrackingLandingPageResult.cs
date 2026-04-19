namespace QPhising.Application.Contracts.Responses.Tracking;

public sealed record TrackingLandingPageResult(
    Guid TrackingPageId,
    string Slug,
    string Title,
    string? Description,
    string DestinationUrl,
    Guid? TemplateId,
    string? TemplateName,
    string? TemplateHtmlContent);
