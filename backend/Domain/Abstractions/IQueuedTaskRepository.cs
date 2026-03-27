using QPhising.Domain.Tasks;

namespace QPhising.Domain.Abstractions;

public interface IQueuedTaskRepository
{
    Task<QueuedTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken = default);

    Task AddAsync(QueuedTask queuedTask, CancellationToken cancellationToken = default);

    void Update(QueuedTask queuedTask);

    Task<int> RequeueExpiredClaimsAsync(DateTimeOffset? asOf = null, CancellationToken cancellationToken = default);

    Task<QueuedTask?> ClaimNextAsync(
        TimeSpan leaseDuration,
        DateTimeOffset? asOf = null,
        CancellationToken cancellationToken = default);
}
