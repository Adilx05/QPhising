using QPhising.Domain.RuntimeConfiguration.Aggregates;

namespace QPhising.Application.Contracts.Abstractions.RuntimeConfiguration;

public interface IRuntimeConfigurationRepository
{
    Task<RuntimeConfigurationAggregate?> GetCurrentAsync(CancellationToken cancellationToken);

    Task SaveAsync(RuntimeConfigurationAggregate aggregate, CancellationToken cancellationToken);
}
