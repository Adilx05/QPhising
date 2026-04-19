namespace QPhising.Api.Infrastructure.Persistence.Entities;

public sealed class VisitEventEntity
{
    public Guid Id { get; set; }

    public Guid TrackingPageId { get; set; }

    public DateTimeOffset OccurredAtUtc { get; set; }

    public string SessionId { get; set; } = string.Empty;

    public string VisitorFingerprint { get; set; } = string.Empty;

    public string? UserAgent { get; set; }

    public string? ReferrerUrl { get; set; }

    public string? IpHash { get; set; }

    public int IpAddressHashPolicy { get; set; }

    public TrackingPageEntity TrackingPage { get; set; } = null!;
}
