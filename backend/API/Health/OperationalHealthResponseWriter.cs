using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace QPhising.Api.Health;

public static class OperationalHealthResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new OperationalHealthResponse(
            OverallStatus: OperationalHealthStatus.ToContractStatus(report.Status),
            LatencyMs: (long)report.TotalDuration.TotalMilliseconds,
            Dependencies: report.Entries
                .Select(static entry => new OperationalHealthDependency(
                    Name: entry.Key,
                    Status: OperationalHealthStatus.ToContractStatus(entry.Value.Status),
                    LatencyMs: (long)entry.Value.Duration.TotalMilliseconds,
                    FailureReason: entry.Value.Exception?.Message ?? entry.Value.Description))
                .OrderBy(static dependency => dependency.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray());

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
