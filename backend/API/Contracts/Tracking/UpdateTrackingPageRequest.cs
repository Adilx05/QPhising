namespace QPhising.Api.Contracts.Tracking;

public sealed record UpdateTrackingPageRequest(
    string Slug,
    string Title,
    string? Description,
    string DestinationUrl,
    int? RetentionDays,
    bool? MaskIpAddress,
    bool? EnableBotFiltering,
    bool? CaptureUtmParameters);
