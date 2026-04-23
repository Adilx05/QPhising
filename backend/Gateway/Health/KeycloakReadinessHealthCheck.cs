using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace QPhising.Gateway.Health;

public sealed class KeycloakReadinessHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var authority = configuration["Authentication:Jwt:Authority"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(authority))
        {
            return HealthCheckResult.Unhealthy("Authentication:Jwt:Authority is missing.");
        }

        var metadataUri = $"{authority}/.well-known/openid-configuration";

        try
        {
            var httpClient = httpClientFactory.CreateClient("health-keycloak");

            using var metadataResponse = await httpClient.GetAsync(metadataUri, cancellationToken);
            if (!metadataResponse.IsSuccessStatusCode)
            {
                return HealthCheckResult.Unhealthy($"Keycloak metadata endpoint returned {(int)metadataResponse.StatusCode}.");
            }

            var metadataJson = await metadataResponse.Content.ReadAsStringAsync(cancellationToken);
            using var metadataDocument = JsonDocument.Parse(metadataJson);
            if (!metadataDocument.RootElement.TryGetProperty("token_endpoint", out var tokenEndpointElement) ||
                tokenEndpointElement.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(tokenEndpointElement.GetString()))
            {
                return HealthCheckResult.Unhealthy("Keycloak metadata is missing token_endpoint.");
            }

            var tokenEndpoint = tokenEndpointElement.GetString()!;
            using var tokenProbe = new HttpRequestMessage(HttpMethod.Head, tokenEndpoint);
            using var tokenResponse = await httpClient.SendAsync(tokenProbe, cancellationToken);

            if ((int)tokenResponse.StatusCode >= 500)
            {
                return HealthCheckResult.Unhealthy($"Keycloak token endpoint returned {(int)tokenResponse.StatusCode}.");
            }

            return HealthCheckResult.Healthy("Keycloak metadata and token endpoint are reachable.");
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("Keycloak readiness probe timed out.", exception);
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Keycloak readiness probe failed.", exception);
        }
    }
}
