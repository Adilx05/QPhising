using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using QPhising.Api.Health;

namespace QPhising.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class HealthController(HealthCheckService healthCheckService) : ControllerBase
{
    [HttpGet(Name = "GetHealth")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var report = await healthCheckService.CheckHealthAsync(
            registration => registration.Tags.Contains("ready"),
            cancellationToken);

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

        var statusCode = report.Status == HealthStatus.Unhealthy
            ? StatusCodes.Status503ServiceUnavailable
            : StatusCodes.Status200OK;

        return StatusCode(statusCode, response);
    }
}
