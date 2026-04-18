using QPhising.Application.Contracts.Abstractions.Persistence;

namespace QPhising.Api.Infrastructure.Persistence;

public sealed class EfCoreUnitOfWork : IUnitOfWork
{
    private readonly QPhisingDbContext _dbContext;

    public EfCoreUnitOfWork(QPhisingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TResponse> ExecuteAsync<TResponse>(
        Func<CancellationToken, Task<TResponse>> operation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var response = await operation(cancellationToken);

        if (_dbContext.ChangeTracker.HasChanges())
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}
