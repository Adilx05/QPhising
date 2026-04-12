using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QPhising.Application.Common.Abstractions.Setup;

namespace QPhising.Infrastructure.Security;

public sealed class SsoSetupValidator(HttpClient httpClient) : ISsoSetupValidator
{
    public async Task<SsoValidationResult> ValidateAsync(SsoValidationInput input, CancellationToken cancellationToken = default)
    {
        string realm = input.Realm.Trim();
        string authority = input.Authority.TrimEnd('/');
        string metadataEndpoint = $"{authority}/realms/{realm}/.well-known/openid-configuration";

        try
        {
            using HttpResponseMessage metadataResponse = await httpClient.GetAsync(metadataEndpoint, cancellationToken);
            if (metadataResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return Failure(
                    "SSO realm was not found.",
                    "realm_not_found",
                    new Dictionary<string, string[]> { ["realm"] = ["Realm does not exist on the provided authority."] });
            }

            if (!metadataResponse.IsSuccessStatusCode)
            {
                return Failure(
                    "Unable to reach realm metadata.",
                    "realm_metadata_unreachable",
                    new Dictionary<string, string[]> { ["authority"] = [$"Metadata endpoint returned {(int)metadataResponse.StatusCode}."] });
            }

            OpenIdMetadataResponse? metadata = await metadataResponse.Content.ReadFromJsonAsync<OpenIdMetadataResponse>(cancellationToken);
            if (metadata is null || string.IsNullOrWhiteSpace(metadata.TokenEndpoint))
            {
                return Failure(
                    "Realm metadata does not expose a token endpoint.",
                    "token_endpoint_missing",
                    new Dictionary<string, string[]> { ["realm"] = ["Token endpoint is missing from OpenID metadata."] });
            }

            using FormUrlEncodedContent tokenRequestPayload = new(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = input.ClientId,
                ["client_secret"] = input.ClientSecret,
                ["audience"] = input.Audience
            });

            using HttpResponseMessage tokenResponse = await httpClient.PostAsync(metadata.TokenEndpoint, tokenRequestPayload, cancellationToken);
            if (tokenResponse.IsSuccessStatusCode)
            {
                return new SsoValidationResult(
                    true,
                    "SSO configuration is valid and token exchange succeeded.",
                    "ready",
                    new Dictionary<string, string[]>());
            }

            string responseBody = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
            TokenErrorResponse tokenError = ParseTokenError(responseBody);
            bool isClientMissing = ContainsClientNotFoundHint(tokenError);

            if (isClientMissing)
            {
                return Failure(
                    "Client was not found in the selected realm.",
                    "client_not_found",
                    new Dictionary<string, string[]> { ["clientId"] = ["Client ID does not exist in this realm."] });
            }

            if (tokenResponse.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
            {
                return Failure(
                    "Client credentials are invalid.",
                    "credentials_invalid",
                    new Dictionary<string, string[]>
                    {
                        ["clientId"] = ["Client authentication failed."],
                        ["clientSecret"] = ["Client authentication failed."]
                    });
            }

            return Failure(
                "Token endpoint validation failed.",
                "token_endpoint_validation_failed",
                new Dictionary<string, string[]>
                {
                    ["authority"] = ["Token endpoint returned an unexpected status code."]
                });
        }
        catch (Exception ex)
        {
            return Failure(
                "SSO validation failed due to a network or protocol error.",
                "sso_validation_exception",
                new Dictionary<string, string[]>
                {
                    ["authority"] = [$"{ex.GetType().Name}: {ex.Message}"]
                });
        }
    }

    private static SsoValidationResult Failure(string message, string technicalReason, IReadOnlyDictionary<string, string[]> fieldErrors)
    {
        return new SsoValidationResult(false, message, technicalReason, fieldErrors);
    }

    private static bool ContainsClientNotFoundHint(TokenErrorResponse tokenError)
    {
        string combined = $"{tokenError.Error} {tokenError.ErrorDescription}";
        return combined.Contains("client", StringComparison.OrdinalIgnoreCase)
            && combined.Contains("not found", StringComparison.OrdinalIgnoreCase);
    }

    private static TokenErrorResponse ParseTokenError(string responseBody)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return new TokenErrorResponse(null, null);
            }

            using JsonDocument document = JsonDocument.Parse(responseBody);
            JsonElement root = document.RootElement;
            string? error = root.TryGetProperty("error", out JsonElement errorElement) ? errorElement.GetString() : null;
            string? errorDescription = root.TryGetProperty("error_description", out JsonElement descriptionElement)
                ? descriptionElement.GetString()
                : null;

            return new TokenErrorResponse(error, errorDescription);
        }
        catch (JsonException)
        {
            return new TokenErrorResponse(null, null);
        }
    }

    private sealed record OpenIdMetadataResponse(string? Issuer, string? TokenEndpoint);

    private sealed record TokenErrorResponse(string? Error, string? ErrorDescription);
}
