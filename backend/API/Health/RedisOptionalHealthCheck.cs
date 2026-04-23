using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace QPhising.Api.Health;

public sealed class RedisOptionalHealthCheck(IConfiguration configuration) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var redisEnabled = configuration.GetValue("HealthChecks:Redis:Enabled", false);
        var redisConnection = configuration["Redis:Configuration"];

        if (!redisEnabled)
        {
            return HealthCheckResult.Degraded("Redis readiness check skipped (optional dependency disabled).");
        }

        if (string.IsNullOrWhiteSpace(redisConnection))
        {
            return HealthCheckResult.Degraded("Redis readiness check skipped (no connection configured).");
        }

        var endpoint = redisConnection.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return HealthCheckResult.Degraded("Redis readiness check skipped (invalid endpoint configuration).");
        }

        var endpointParts = endpoint.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var host = endpointParts[0];
        var port = endpointParts.Length > 1 && int.TryParse(endpointParts[1], out var parsedPort) ? parsedPort : 6379;

        try
        {
            using var client = new TcpClient();
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(2));

            await client.ConnectAsync(host, port, timeoutCts.Token);
            return HealthCheckResult.Healthy($"Redis TCP connectivity succeeded ({host}:{port}).");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Degraded("Redis readiness probe failed (optional dependency).", exception);
        }
    }
}
