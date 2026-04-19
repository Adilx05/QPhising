namespace QPhising.Api.Contracts.Tracking;

public sealed record CreateTrackingPageRequest(
    string Slug,
    string Title,
    string? Description,
    string? OwnerId,
    Guid? TemplateId,
    string? CustomHtmlContent,
    DateTimeOffset? ValidFromUtc,
    DateTimeOffset? ValidUntilUtc,
    int? RetentionDays,
    bool? MaskIpAddress,
    bool? EnableBotFiltering,
    bool? CaptureUtmParameters);
