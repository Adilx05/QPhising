using QPhising.Domain.Tasks;

namespace QPhising.Worker.TaskExecution;

public interface IQueuedTaskHandlerRegistry
{
    bool TryResolve(TaskType taskType, out IQueuedTaskHandler handler);
}
