using QPhising.Application.Contracts.Abstractions.Persistence;
using System.Threading;

namespace QPhising.Api.Services.Persistence;

public sealed class FileBackedUnitOfWork : IUnitOfWork
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    public async Task<TResponse> ExecuteAsync<TResponse>(
        Func<CancellationToken, Task<TResponse>> operation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await Gate.WaitAsync(cancellationToken);

        try
        {
            return await operation(cancellationToken);
        }
        finally
        {
            Gate.Release();
        }
    }
}
