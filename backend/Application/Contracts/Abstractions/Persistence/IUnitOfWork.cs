namespace QPhising.Application.Contracts.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<TResponse> ExecuteAsync<TResponse>(
        Func<CancellationToken, Task<TResponse>> operation,
        CancellationToken cancellationToken);
}
