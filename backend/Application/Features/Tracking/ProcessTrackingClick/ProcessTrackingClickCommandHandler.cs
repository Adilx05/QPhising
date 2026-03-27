using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Tracking;

namespace QPhising.Application.Features.Tracking.ProcessTrackingClick;

public sealed class ProcessTrackingClickCommandHandler(
    ICampaignInteractionGuard campaignInteractionGuard,
    ITrackingTokenService trackingTokenService,
    ITrackingClickRepository trackingClickRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ProcessTrackingClickCommand, Result<ProcessTrackingClickResponse>>
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

        DateTimeOffset clickedAtUtc = request.ClickedAtUtc ?? DateTimeOffset.UtcNow;
        TrackingClick clickEvent = TrackingClick.Create(
            request.CampaignId,
            request.TrackingToken,
            request.IpAddress,
            request.UserAgent,
            clickedAtUtc,
            request.Fingerprint);

        await trackingClickRepository.AddAsync(clickEvent, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ProcessTrackingClickResponse response = new(
            clickEvent.Id,
            request.CampaignId,
            request.TrackingToken,
            clickEvent.IpAddress,
            clickEvent.UserAgent,
            clickEvent.Fingerprint,
            clickedAtUtc,
            Accepted: true);

        return Result<ProcessTrackingClickResponse>.Success(response);
    }
}
