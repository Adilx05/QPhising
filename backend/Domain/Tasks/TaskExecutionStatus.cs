namespace QPhising.Domain.Tasks;

public enum TaskExecutionStatus
{
    Queued = 1,
    Claimed = 2,
    Running = 3,
    Succeeded = 4,
    Failed = 5,
    DeadLettered = 6,
    Canceled = 7
}
