using MediatR;
using QPhising.Application.Common;

namespace QPhising.Application.Features.Tracking.ProcessTrackingClick;

public sealed record ProcessTrackingClickCommand(
    Guid CampaignId,
    string TrackingToken,
    string IpAddress,
    string UserAgent,
    string? Fingerprint = null,
    DateTimeOffset? ClickedAtUtc = null) : IRequest<Result<ProcessTrackingClickResponse>>;

public sealed record ProcessTrackingClickResponse(
    Guid ClickId,
    Guid CampaignId,
    string TrackingToken,
    string IpAddress,
    string UserAgent,
    string? Fingerprint,
    DateTimeOffset ClickedAtUtc,
    bool Accepted);
