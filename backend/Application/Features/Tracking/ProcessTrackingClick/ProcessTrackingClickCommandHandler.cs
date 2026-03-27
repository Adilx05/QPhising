using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions;

namespace QPhising.Application.Features.Tracking.ProcessTrackingClick;

public sealed class ProcessTrackingClickCommandHandler(
    ICampaignInteractionGuard campaignInteractionGuard,
    ITrackingTokenService trackingTokenService) : IRequestHandler<ProcessTrackingClickCommand, Result<ProcessTrackingClickResponse>>
{
    public async Task<Result<ProcessTrackingClickResponse>> Handle(ProcessTrackingClickCommand request, CancellationToken cancellationToken)
    {
        var guardResult = await campaignInteractionGuard.EnsureTrackingInteractionAllowedAsync(request.CampaignId, cancellationToken);
        if (!guardResult.IsSuccess)
        {
            return Result<ProcessTrackingClickResponse>.Failure(guardResult.Errors.ToArray());
        }

        var validationResult = trackingTokenService.ValidateToken(request.TrackingToken, request.CampaignId);
        if (!validationResult.IsValid)
        {
            string errorMessage = validationResult.Failure switch
            {
                TrackingTokenValidationFailure.Expired => "Tracking token has expired.",
                TrackingTokenValidationFailure.CampaignMismatch => "Tracking token campaign mismatch.",
                TrackingTokenValidationFailure.SignatureMismatch => "Tracking token signature is invalid.",
                _ => "Tracking token is malformed or unsupported."
            };

            return Result<ProcessTrackingClickResponse>.Failure(errorMessage);
        }

        ProcessTrackingClickResponse response = new(
            request.CampaignId,
            request.TrackingToken,
            request.ClickedAtUtc ?? DateTimeOffset.UtcNow,
            Accepted: true);

        return Result<ProcessTrackingClickResponse>.Success(response);
    }
}
