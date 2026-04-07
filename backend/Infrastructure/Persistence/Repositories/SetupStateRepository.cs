using Microsoft.EntityFrameworkCore;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Setup;

namespace QPhising.Infrastructure.Persistence.Repositories;

public sealed class SetupStateRepository(QPhisingDbContext dbContext) : ISetupStateRepository
{
    public Task<SetupState?> GetAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SetupStates.AsNoTracking().SingleOrDefaultAsync(cancellationToken);
    }

    public Task AddAsync(SetupState setupState, CancellationToken cancellationToken = default)
    {
        return dbContext.SetupStates.AddAsync(setupState, cancellationToken).AsTask();
    }

    public void Update(SetupState setupState)
    {
        dbContext.SetupStates.Update(setupState);
    }
}
