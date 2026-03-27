namespace QPhising.Application.Common.Abstractions;

public interface ITrackingTokenService
{
    TrackingTokenIssueResult IssueToken(TrackingTokenIssueRequest request);

    TrackingTokenValidationResult ValidateToken(string token, Guid expectedCampaignId);
}

public sealed record TrackingTokenIssueRequest(
    Guid CampaignId,
    string RecipientEmail,
    string Nonce);

public sealed record TrackingTokenIssueResult(
    string Token,
    DateTimeOffset IssuedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    string SignatureAlgorithm,
    int Version);

public sealed record TrackingTokenClaims(
    Guid CampaignId,
    string RecipientEmail,
    DateTimeOffset IssuedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    string Nonce,
    int Version);

public sealed record TrackingTokenValidationResult(
    bool IsValid,
    TrackingTokenValidationFailure? Failure,
    TrackingTokenClaims? Claims)
{
    public static TrackingTokenValidationResult Success(TrackingTokenClaims claims) =>
        new(true, null, claims);

    public static TrackingTokenValidationResult Invalid(TrackingTokenValidationFailure failure) =>
        new(false, failure, null);
}

public enum TrackingTokenValidationFailure
{
    Malformed,
    UnsupportedVersion,
    SignatureMismatch,
    Expired,
    CampaignMismatch,
    InvalidIssuedAt,
    InvalidRecipient,
    InvalidNonce
}
