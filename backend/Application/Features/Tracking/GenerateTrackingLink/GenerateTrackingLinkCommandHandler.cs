using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions;

namespace QPhising.Application.Features.Tracking.GenerateTrackingLink;

public sealed class GenerateTrackingLinkCommandHandler(
    ICampaignInteractionGuard campaignInteractionGuard) : IRequestHandler<GenerateTrackingLinkCommand, Result<GenerateTrackingLinkResponse>>
{
    public async Task<Result<GenerateTrackingLinkResponse>> Handle(GenerateTrackingLinkCommand request, CancellationToken cancellationToken)
    {
        var guardResult = await campaignInteractionGuard.EnsureTrackingInteractionAllowedAsync(request.CampaignId, cancellationToken);
        if (!guardResult.IsSuccess)
        {
            return Result<GenerateTrackingLinkResponse>.Failure(guardResult.Errors.ToArray());
        }

        string token = Guid.NewGuid().ToString("N");
        DateTimeOffset generatedAt = DateTimeOffset.UtcNow;

        GenerateTrackingLinkResponse response = new(
            request.CampaignId,
            request.RecipientEmail,
            token,
            $"/api/v1/tracking/click/{request.CampaignId}/{token}",
            generatedAt);

        return Result<GenerateTrackingLinkResponse>.Success(response);
    }
}
