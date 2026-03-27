namespace QPhising.Domain.Tracking;

public sealed class TrackingClick
{
    private const int MaxUserAgentLength = 1024;
    private const int MaxFingerprintLength = 256;

    private TrackingClick(
        Guid id,
        Guid campaignId,
        string trackingToken,
        string ipAddress,
        string userAgent,
        DateTimeOffset clickedAtUtc,
        string? fingerprint)
    {
        Id = id;
        CampaignId = campaignId;
        TrackingToken = trackingToken;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        ClickedAtUtc = clickedAtUtc;
        Fingerprint = fingerprint;
    }

    public Guid Id { get; }

    public Guid CampaignId { get; }

    public string TrackingToken { get; }

    public string IpAddress { get; }

    public string UserAgent { get; }

    public DateTimeOffset ClickedAtUtc { get; }

    public string? Fingerprint { get; }

    public static TrackingClick Create(
        Guid campaignId,
        string trackingToken,
        string ipAddress,
        string userAgent,
        DateTimeOffset clickedAtUtc,
        string? fingerprint,
        Guid? id = null)
    {
        if (campaignId == Guid.Empty)
        {
            throw new ArgumentException("CampaignId is required.", nameof(campaignId));
        }

        if (string.IsNullOrWhiteSpace(trackingToken))
        {
            throw new ArgumentException("TrackingToken is required.", nameof(trackingToken));
        }

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            throw new ArgumentException("IpAddress is required.", nameof(ipAddress));
        }

        if (string.IsNullOrWhiteSpace(userAgent))
        {
            throw new ArgumentException("UserAgent is required.", nameof(userAgent));
        }

        if (userAgent.Length > MaxUserAgentLength)
        {
            throw new ArgumentException($"UserAgent must be {MaxUserAgentLength} characters or fewer.", nameof(userAgent));
        }

        if (!string.IsNullOrWhiteSpace(fingerprint) && fingerprint.Length > MaxFingerprintLength)
        {
            throw new ArgumentException($"Fingerprint must be {MaxFingerprintLength} characters or fewer.", nameof(fingerprint));
        }

        return new TrackingClick(
            id ?? Guid.NewGuid(),
            campaignId,
            trackingToken.Trim(),
            ipAddress.Trim(),
            userAgent.Trim(),
            clickedAtUtc,
            string.IsNullOrWhiteSpace(fingerprint) ? null : fingerprint.Trim());
    }
}
