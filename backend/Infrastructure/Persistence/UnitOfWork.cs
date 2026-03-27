using QPhising.Domain.Abstractions;

namespace QPhising.Infrastructure.Persistence;

public sealed class UnitOfWork(QPhisingDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
