using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions;

namespace QPhising.Application.Features.Tracking.ProcessTrackingClick;

public sealed class ProcessTrackingClickCommandHandler(
    ICampaignInteractionGuard campaignInteractionGuard) : IRequestHandler<ProcessTrackingClickCommand, Result<ProcessTrackingClickResponse>>
{
    public async Task<Result<ProcessTrackingClickResponse>> Handle(ProcessTrackingClickCommand request, CancellationToken cancellationToken)
    {
        var guardResult = await campaignInteractionGuard.EnsureTrackingInteractionAllowedAsync(request.CampaignId, cancellationToken);
        if (!guardResult.IsSuccess)
        {
            return Result<ProcessTrackingClickResponse>.Failure(guardResult.Errors.ToArray());
        }

        ProcessTrackingClickResponse response = new(
            request.CampaignId,
            request.TrackingToken,
            request.ClickedAtUtc ?? DateTimeOffset.UtcNow,
            Accepted: true);

        return Result<ProcessTrackingClickResponse>.Success(response);
    }
}
