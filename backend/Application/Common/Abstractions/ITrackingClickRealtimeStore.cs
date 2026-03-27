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
    DateTimeOffset ClickedAtUtc);

public sealed record TrackingClickRealtimeResult(
    bool IsDuplicate,
    long CampaignClickCount,
    long RecipientClickCount);
