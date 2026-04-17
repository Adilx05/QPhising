using QPhising.Domain.Setup.Aggregates;

namespace QPhising.Application.Contracts.Abstractions.Setup;

public interface ISetupConfigurationRepository
{
    Task<SetupAggregate?> GetCurrentAsync(CancellationToken cancellationToken);

    Task SaveAsync(SetupAggregate aggregate, CancellationToken cancellationToken);
}
