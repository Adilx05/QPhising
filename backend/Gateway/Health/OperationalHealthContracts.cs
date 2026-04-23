using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace QPhising.Gateway.Health;

public sealed record OperationalHealthDependency(
    string Name,
    string Status,
    long LatencyMs,
    string? FailureReason);

public sealed record OperationalHealthResponse(
    string OverallStatus,
    long LatencyMs,
    IReadOnlyCollection<OperationalHealthDependency> Dependencies);

public static class OperationalHealthStatus
{
    public static string ToContractStatus(HealthStatus status)
    {
        return status switch
        {
            HealthStatus.Healthy => "Healthy",
            HealthStatus.Degraded => "Degraded",
            HealthStatus.Unhealthy => "Unhealthy",
            _ => "Unknown"
        };
    }
}
