using QPhising.Domain.Tasks;

namespace QPhising.Worker.TaskExecution;

public interface IQueuedTaskHandler
{
    TaskType TaskType { get; }

    Task<QueuedTaskHandlerResult> HandleAsync(QueuedTask queuedTask, CancellationToken cancellationToken);
}
