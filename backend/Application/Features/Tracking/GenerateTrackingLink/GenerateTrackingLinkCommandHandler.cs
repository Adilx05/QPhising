using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions;

namespace QPhising.Application.Features.Tracking.GenerateTrackingLink;

public sealed class GenerateTrackingLinkCommandHandler(
    ICampaignInteractionGuard campaignInteractionGuard,
    ITrackingTokenService trackingTokenService) : IRequestHandler<GenerateTrackingLinkCommand, Result<GenerateTrackingLinkResponse>>
{
    public async Task<Result<GenerateTrackingLinkResponse>> Handle(GenerateTrackingLinkCommand request, CancellationToken cancellationToken)
    {
        var guardResult = await campaignInteractionGuard.EnsureTrackingInteractionAllowedAsync(request.CampaignId, cancellationToken);
        if (!guardResult.IsSuccess)
        {
            return Result<GenerateTrackingLinkResponse>.Failure(guardResult.Errors.ToArray());
        }

        var issueResult = trackingTokenService.IssueToken(new TrackingTokenIssueRequest(
            request.CampaignId,
            request.RecipientEmail,
            Guid.NewGuid().ToString("N")));

        GenerateTrackingLinkResponse response = new(
            request.CampaignId,
            request.RecipientEmail,
            issueResult.Token,
            $"/api/v1/tracking/click/{request.CampaignId}/{issueResult.Token}",
            issueResult.IssuedAtUtc,
            issueResult.ExpiresAtUtc,
            issueResult.SignatureAlgorithm,
            issueResult.Version);

        return Result<GenerateTrackingLinkResponse>.Success(response);
    }
}
