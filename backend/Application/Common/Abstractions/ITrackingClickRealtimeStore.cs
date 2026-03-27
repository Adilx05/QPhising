namespace QPhising.Application.Common.Abstractions;

public interface ITrackingClickRealtimeStore
{
    Task<TrackingClickRealtimeResult> RegisterClickAsync(
        TrackingClickRealtimeRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record TrackingClickRealtimeRequest(
    Guid CampaignId,
    string RecipientEmail,
    string TokenNonce,
    DateTimeOffset TokenIssuedAtUtc,
    DateTimeOffset TokenExpiresAtUtc,
    DateTimeOffset ClickedAtUtc,
    string IpAddress,
    string? Fingerprint);

public sealed record TrackingClickRealtimeResult(
    bool IsDuplicate,
    bool IsRejected,
    bool IsFlagged,
    string? DecisionReason,
    long CampaignClickCount,
    long RecipientClickCount);
