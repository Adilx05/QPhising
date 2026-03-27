using QPhising.Domain.Tasks;

namespace QPhising.Worker.TaskExecution.Handlers;

public sealed class ExportGenerationTaskHandler : IQueuedTaskHandler
{
    public TaskType TaskType => TaskType.ExportGeneration;

    public Task<QueuedTaskHandlerResult> HandleAsync(QueuedTask queuedTask, CancellationToken cancellationToken)
    {
        return Task.FromResult(QueuedTaskHandlerResult.Failure(
            "Export generation task execution is not available until export subsystem delivery is completed.",
            isRetryable: false));
    }
}
