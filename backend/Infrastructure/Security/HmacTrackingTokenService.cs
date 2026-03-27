using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using QPhising.Application.Common.Abstractions;

namespace QPhising.Infrastructure.Security;

public sealed class HmacTrackingTokenService(IOptions<TrackingTokenOptions> options) : ITrackingTokenService
{
    private const string Algorithm = "HS256";

    private readonly TrackingTokenOptions _options = options.Value;

    public TrackingTokenIssueResult IssueToken(TrackingTokenIssueRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RecipientEmail))
        {
            throw new ArgumentException("Recipient email is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Nonce))
        {
            throw new ArgumentException("Nonce is required.", nameof(request));
        }

        DateTimeOffset issuedAtUtc = DateTimeOffset.UtcNow;
        DateTimeOffset expiresAtUtc = issuedAtUtc.AddMinutes(_options.ExpirationMinutes);

        byte[] headerBytes = JsonSerializer.SerializeToUtf8Bytes(new TrackingTokenHeader(Algorithm, "QPT", _options.Version));
        byte[] payloadBytes = JsonSerializer.SerializeToUtf8Bytes(new TrackingTokenPayload(
            request.CampaignId,
            request.RecipientEmail.Trim().ToLowerInvariant(),
            issuedAtUtc.ToUnixTimeSeconds(),
            expiresAtUtc.ToUnixTimeSeconds(),
            request.Nonce,
            _options.Version));

        string encodedHeader = Base64UrlEncode(headerBytes);
        string encodedPayload = Base64UrlEncode(payloadBytes);

        string unsignedToken = $"{encodedHeader}.{encodedPayload}";
        string signature = CreateSignature(unsignedToken);

        return new TrackingTokenIssueResult(
            $"{unsignedToken}.{signature}",
            issuedAtUtc,
            expiresAtUtc,
            Algorithm,
            _options.Version);
    }

    public TrackingTokenValidationResult ValidateToken(string token, Guid expectedCampaignId)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return TrackingTokenValidationResult.Invalid(TrackingTokenValidationFailure.Malformed);
        }

        string[] segments = token.Split('.');
        if (segments.Length != 3)
        {
            return TrackingTokenValidationResult.Invalid(TrackingTokenValidationFailure.Malformed);
        }

        string unsignedToken = $"{segments[0]}.{segments[1]}";
        string expectedSignature = CreateSignature(unsignedToken);

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedSignature),
                Encoding.UTF8.GetBytes(segments[2])))
        {
            return TrackingTokenValidationResult.Invalid(TrackingTokenValidationFailure.SignatureMismatch);
        }

        TrackingTokenHeader? header;
        TrackingTokenPayload? payload;

        try
        {
            header = JsonSerializer.Deserialize<TrackingTokenHeader>(Base64UrlDecode(segments[0]));
            payload = JsonSerializer.Deserialize<TrackingTokenPayload>(Base64UrlDecode(segments[1]));
        }
        catch (Exception)
        {
            return TrackingTokenValidationResult.Invalid(TrackingTokenValidationFailure.Malformed);
        }

        if (header is null || payload is null)
        {
            return TrackingTokenValidationResult.Invalid(TrackingTokenValidationFailure.Malformed);
        }

        if (!string.Equals(header.Algorithm, Algorithm, StringComparison.Ordinal) ||
            !string.Equals(header.Type, "QPT", StringComparison.Ordinal))
        {
            return TrackingTokenValidationResult.Invalid(TrackingTokenValidationFailure.Malformed);
        }

        if (header.Version != _options.Version || payload.Version != _options.Version)
        {
            return TrackingTokenValidationResult.Invalid(TrackingTokenValidationFailure.UnsupportedVersion);
        }

        if (payload.CampaignId != expectedCampaignId)
        {
            return TrackingTokenValidationResult.Invalid(TrackingTokenValidationFailure.CampaignMismatch);
        }

        if (payload.ExpiresAtUnixSeconds <= payload.IssuedAtUnixSeconds)
        {
            return TrackingTokenValidationResult.Invalid(TrackingTokenValidationFailure.InvalidIssuedAt);
        }

        DateTimeOffset expiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(payload.ExpiresAtUnixSeconds);
        if (expiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return TrackingTokenValidationResult.Invalid(TrackingTokenValidationFailure.Expired);
        }

        if (string.IsNullOrWhiteSpace(payload.RecipientEmail))
        {
            return TrackingTokenValidationResult.Invalid(TrackingTokenValidationFailure.InvalidRecipient);
        }

        if (string.IsNullOrWhiteSpace(payload.Nonce))
        {
            return TrackingTokenValidationResult.Invalid(TrackingTokenValidationFailure.InvalidNonce);
        }

        var claims = new TrackingTokenClaims(
            payload.CampaignId,
            payload.RecipientEmail,
            DateTimeOffset.FromUnixTimeSeconds(payload.IssuedAtUnixSeconds),
            expiresAtUtc,
            payload.Nonce,
            payload.Version);

        return TrackingTokenValidationResult.Success(claims);
    }

    private string CreateSignature(string unsignedToken)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(_options.SigningKey);
        using var hmac = new HMACSHA256(keyBytes);
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(unsignedToken));
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] value) =>
        Convert.ToBase64String(value)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        string normalized = value.Replace('-', '+').Replace('_', '/');
        int padding = 4 - normalized.Length % 4;
        if (padding is > 0 and < 4)
        {
            normalized = normalized.PadRight(normalized.Length + padding, '=');
        }

        return Convert.FromBase64String(normalized);
    }

    private sealed record TrackingTokenHeader(string Algorithm, string Type, int Version);

    private sealed record TrackingTokenPayload(
        Guid CampaignId,
        string RecipientEmail,
        long IssuedAtUnixSeconds,
        long ExpiresAtUnixSeconds,
        string Nonce,
        int Version);
}
