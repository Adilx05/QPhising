using QPhising.Domain.Tracking.Enums;

namespace QPhising.Api.Contracts.Tracking;

public sealed record CaptureVisitRequest(
    DateTimeOffset OccurredAtUtc,
    string SessionId,
    string VisitorFingerprint,
    string? UserAgent,
    string? ReferrerUrl,
    string? IpHash,
    IpAddressHashPolicy IpAddressHashPolicy,
    int DeduplicationWindowSeconds = 120);
