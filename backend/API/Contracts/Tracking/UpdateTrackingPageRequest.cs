namespace QPhising.Api.Contracts.Tracking;

public sealed record UpdateTrackingPageRequest(
    string Slug,
    string Title,
    string? Description,
    string DestinationUrl,
    Guid? TemplateId,
    int? RetentionDays,
    bool? MaskIpAddress,
    bool? EnableBotFiltering,
    bool? CaptureUtmParameters);
