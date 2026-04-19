using QPhising.Domain.Tracking.Enums;

namespace QPhising.Application.Contracts.Responses.Tracking;

public sealed record TrackingLandingPageResult(
    Guid TrackingPageId,
    string Slug,
    string Title,
    string? Description,
    Guid? TemplateId,
    string? TemplateName,
    string? TemplateHtmlContent,
    string? CustomHtmlContent,
    DateTimeOffset? ValidFromUtc,
    DateTimeOffset? ValidUntilUtc,
    bool CaptureIpAddress,
    IpAddressHashPolicy IpAddressHashPolicy);
