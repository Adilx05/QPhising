using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace QPhising.Infrastructure.Persistence;

public sealed class InfrastructureOptionsHealthCheck : IHealthCheck
{
    private readonly InfrastructureOptions _options;

    public InfrastructureOptionsHealthCheck(IOptions<InfrastructureOptions> options)
    {
        _options = options.Value;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var valid = !string.IsNullOrWhiteSpace(_options.ConnectionString) &&
                    !string.IsNullOrWhiteSpace(_options.RedisConnectionString);

        return Task.FromResult(valid
            ? HealthCheckResult.Healthy("Infrastructure options are loaded.")
            : HealthCheckResult.Unhealthy("Invalid infrastructure configuration."));
    }
}
