using QPhising.Domain.Tasks;

namespace QPhising.Worker.TaskExecution;

public interface IQueuedTaskDispatcher
{
    Task<QueuedTaskHandlerResult> DispatchAsync(QueuedTask queuedTask, CancellationToken cancellationToken);
}
