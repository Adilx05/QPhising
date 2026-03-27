using MediatR;
using QPhising.Application.Common;

namespace QPhising.Application.Features.Tracking.GenerateTrackingLink;

public sealed record GenerateTrackingLinkCommand(
    Guid CampaignId,
    string RecipientEmail) : IRequest<Result<GenerateTrackingLinkResponse>>;

public sealed record GenerateTrackingLinkResponse(
    Guid CampaignId,
    string RecipientEmail,
    string TrackingToken,
    string TrackingPath,
    DateTimeOffset GeneratedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    string SignatureAlgorithm,
    int TokenVersion);
