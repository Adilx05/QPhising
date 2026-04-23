using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace QPhising.Gateway.Health;

public sealed class DownstreamApiReadinessHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var enabled = configuration.GetValue("HealthChecks:DownstreamApi:Enabled", true);
        if (!enabled)
        {
            return HealthCheckResult.Degraded("Downstream API readiness check skipped (disabled).");
        }

        var endpointPath = configuration["HealthChecks:DownstreamApi:Endpoint"] ?? "/health/ready";
        var host = configuration["HealthChecks:DownstreamApi:Host"] ?? "localhost";
        var port = configuration.GetValue("HealthChecks:DownstreamApi:Port", 5050);
        var scheme = configuration["HealthChecks:DownstreamApi:Scheme"] ?? "http";
        var endpoint = new Uri($"{scheme}://{host}:{port}{endpointPath}");

        try
        {
            var client = httpClientFactory.CreateClient("health-downstream-api");
            using var response = await client.GetAsync(endpoint, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Unhealthy($"Downstream API readiness endpoint returned {(int)response.StatusCode}.");
            }

            return HealthCheckResult.Healthy("Downstream API readiness endpoint is reachable.");
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("Downstream API readiness probe timed out.", exception);
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Downstream API readiness probe failed.", exception);
        }
    }
}
