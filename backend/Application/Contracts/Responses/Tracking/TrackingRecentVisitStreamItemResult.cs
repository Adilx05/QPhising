using QPhising.Domain.Tracking.Enums;

namespace QPhising.Application.Contracts.Responses.Tracking;

public sealed record TrackingRecentVisitStreamItemResult(
    Guid VisitId,
    Guid TrackingPageId,
    string TrackingPageSlug,
    DateTimeOffset OccurredAtUtc,
    string SessionId,
    string VisitorFingerprint,
    string? UserAgent,
    string? ReferrerUrl,
    string? IpHash,
    IpAddressHashPolicy IpAddressHashPolicy);
