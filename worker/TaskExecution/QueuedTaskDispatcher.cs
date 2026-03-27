using QPhising.Domain.Tasks;

namespace QPhising.Worker.TaskExecution;

public sealed class QueuedTaskDispatcher(
    IQueuedTaskHandlerRegistry handlerRegistry,
    ILogger<QueuedTaskDispatcher> logger) : IQueuedTaskDispatcher
{
    public async Task<QueuedTaskHandlerResult> DispatchAsync(QueuedTask queuedTask, CancellationToken cancellationToken)
    {
        if (!handlerRegistry.TryResolve(queuedTask.Type, out IQueuedTaskHandler? handler))
        {
            logger.LogError("No task handler is registered for type {TaskType}. TaskId={TaskId}", queuedTask.Type, queuedTask.Id);
            return QueuedTaskHandlerResult.Failure(
                $"No handler registered for task type '{queuedTask.Type}'.",
                isRetryable: false);
        }

        return await handler.HandleAsync(queuedTask, cancellationToken);
    }
}
