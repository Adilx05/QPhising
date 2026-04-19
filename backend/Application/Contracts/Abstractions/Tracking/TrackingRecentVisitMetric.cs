using QPhising.Domain.Tracking.Enums;

namespace QPhising.Application.Contracts.Abstractions.Tracking;

public sealed record TrackingRecentVisitMetric(
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
