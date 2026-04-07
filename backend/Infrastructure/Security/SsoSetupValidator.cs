using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using QPhising.Application.Common.Abstractions.Setup;

namespace QPhising.Infrastructure.Security;

public sealed class SsoSetupValidator(HttpClient httpClient, IOptions<KeycloakValidationOptions> options) : ISsoSetupValidator
{
    public async Task<(bool IsValid, string Message)> ValidateAsync(CancellationToken cancellationToken = default)
    {
        string metadataEndpoint = $"{options.Value.Authority.TrimEnd('/')}/.well-known/openid-configuration";

        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(metadataEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return (false, $"SSO metadata endpoint returned {(int)response.StatusCode}.");
            }

            OpenIdMetadataResponse? metadata = await response.Content.ReadFromJsonAsync<OpenIdMetadataResponse>(cancellationToken);
            if (metadata is null || string.IsNullOrWhiteSpace(metadata.Issuer))
            {
                return (false, "SSO metadata payload is invalid.");
            }

            return (true, "SSO configuration is reachable and valid.");
        }
        catch (Exception ex)
        {
            return (false, $"SSO validation failed: {ex.Message}");
        }
    }

    private sealed record OpenIdMetadataResponse(string Issuer);
}
