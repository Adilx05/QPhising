using QPhising.Domain.Setup;

namespace QPhising.Domain.Abstractions;

public interface ISetupStateRepository
{
    Task<SetupState?> GetAsync(CancellationToken cancellationToken = default);

    Task AddAsync(SetupState setupState, CancellationToken cancellationToken = default);

    void Update(SetupState setupState);
}
