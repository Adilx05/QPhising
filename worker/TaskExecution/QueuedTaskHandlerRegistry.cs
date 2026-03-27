using QPhising.Domain.Tasks;

namespace QPhising.Worker.TaskExecution;

public sealed class QueuedTaskHandlerRegistry : IQueuedTaskHandlerRegistry
{
    private readonly IReadOnlyDictionary<TaskType, IQueuedTaskHandler> _handlers;

    public QueuedTaskHandlerRegistry(IEnumerable<IQueuedTaskHandler> handlers)
    {
        Dictionary<TaskType, IQueuedTaskHandler> index = [];

        foreach (IQueuedTaskHandler handler in handlers)
        {
            if (!index.TryAdd(handler.TaskType, handler))
            {
                throw new InvalidOperationException($"Multiple handlers are registered for task type '{handler.TaskType}'.");
            }
        }

        _handlers = index;
    }

    public bool TryResolve(TaskType taskType, out IQueuedTaskHandler handler)
    {
        return _handlers.TryGetValue(taskType, out handler!);
    }
}
