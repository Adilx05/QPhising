using QPhising.Domain.Tasks;

namespace QPhising.Domain.Abstractions;

public interface ITaskExecutionLogRepository
{
    Task AddAsync(TaskExecutionLog executionLog, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TaskExecutionLog>> ListByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);
}
