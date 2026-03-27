using QPhising.Domain.Abstractions;

namespace QPhising.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }
}
