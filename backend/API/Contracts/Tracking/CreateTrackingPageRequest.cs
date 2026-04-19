namespace QPhising.Api.Contracts.Tracking;

public sealed record CreateTrackingPageRequest(
    string Slug,
    string Title,
    string? Description,
    string DestinationUrl,
    string? OwnerId,
    Guid? TemplateId,
    int? RetentionDays,
    bool? MaskIpAddress,
    bool? EnableBotFiltering,
    bool? CaptureUtmParameters);
