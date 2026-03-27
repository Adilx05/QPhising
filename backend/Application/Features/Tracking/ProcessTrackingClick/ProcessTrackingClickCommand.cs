using MediatR;
using QPhising.Application.Common;

namespace QPhising.Application.Features.Tracking.ProcessTrackingClick;

public sealed record ProcessTrackingClickCommand(
    Guid CampaignId,
    string TrackingToken,
    DateTimeOffset? ClickedAtUtc = null) : IRequest<Result<ProcessTrackingClickResponse>>;

public sealed record ProcessTrackingClickResponse(
    Guid CampaignId,
    string TrackingToken,
    DateTimeOffset ClickedAtUtc,
    bool Accepted);
