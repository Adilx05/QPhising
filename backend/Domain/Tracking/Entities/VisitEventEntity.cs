using QPhising.Domain.Common;
using QPhising.Domain.Tracking.Enums;
using QPhising.Domain.Tracking.ValueObjects;

namespace QPhising.Domain.Tracking.Entities;

public sealed class VisitEventEntity : Entity<Guid>
{
    public const int MaxUserAgentLength = 1024;
    public const int MaxReferrerLength = 2048;
    public const int MaxIpHashLength = 128;

    public VisitEventEntity(
        Guid id,
        Guid trackingPageId,
        DateTimeOffset occurredAtUtc,
        TrackingIdentifier sessionId,
        TrackingIdentifier visitorFingerprint,
        string? userAgent,
        string? referrerUrl,
        string? ipHash,
        IpAddressHashPolicy ipAddressHashPolicy)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(sessionId);
        ArgumentNullException.ThrowIfNull(visitorFingerprint);

        if (trackingPageId == Guid.Empty)
        {
            throw new ArgumentException("Tracking page ID is required.", nameof(trackingPageId));
        }

        TrackingPageId = trackingPageId;
        OccurredAtUtc = occurredAtUtc;
        SessionId = sessionId;
        VisitorFingerprint = visitorFingerprint;
        UserAgent = NormalizeOptional(userAgent, MaxUserAgentLength, nameof(userAgent));
        ReferrerUrl = NormalizeOptional(referrerUrl, MaxReferrerLength, nameof(referrerUrl));
        IpHash = NormalizeIpHash(ipHash, ipAddressHashPolicy);
        IpAddressHashPolicy = ipAddressHashPolicy;
    }

    public Guid TrackingPageId { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    public TrackingIdentifier SessionId { get; }

    public TrackingIdentifier VisitorFingerprint { get; }

    public string? UserAgent { get; }

    public string? ReferrerUrl { get; }

    public string? IpHash { get; }

    public IpAddressHashPolicy IpAddressHashPolicy { get; }

    private static string? NormalizeIpHash(string? value, IpAddressHashPolicy policy)
    {
        if (policy == IpAddressHashPolicy.None)
        {
            return null;
        }

        var normalized = NormalizeOptional(value, MaxIpHashLength, nameof(value));
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("IP hash is required when a hash policy is enabled.", nameof(value));
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", parameterName);
        }

        return normalized;
    }
}
