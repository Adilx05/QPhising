using QPhising.Domain.Tasks;

namespace QPhising.Domain.Tasks.Exceptions;

public sealed class InvalidTaskStatusTransitionException : TaskDomainException
{
    public InvalidTaskStatusTransitionException(TaskExecutionStatus fromStatus, TaskExecutionStatus toStatus)
        : base($"Task status transition from '{fromStatus}' to '{toStatus}' is not allowed.")
    {
        FromStatus = fromStatus;
        ToStatus = toStatus;
    }

    public TaskExecutionStatus FromStatus { get; }

    public TaskExecutionStatus ToStatus { get; }
}
