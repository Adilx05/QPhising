namespace QPhising.Api.Contracts.Tracking;

public sealed record UpdateTrackingPageRequest(
    string Slug,
    string Title,
    string? Description,
    Guid? TemplateId,
    string? CustomHtmlContent,
    DateTimeOffset? ValidFromUtc,
    DateTimeOffset? ValidUntilUtc,
    int? RetentionDays,
    bool? MaskIpAddress,
    bool? EnableBotFiltering,
    bool? CaptureUtmParameters);
