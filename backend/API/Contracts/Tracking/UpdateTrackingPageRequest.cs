using QPhising.Domain.Tracking.Enums;

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
    bool? CaptureIpAddress,
    IpAddressHashPolicy? IpAddressHashPolicy,
    bool? EnableBotFiltering,
    bool? CaptureUtmParameters);
