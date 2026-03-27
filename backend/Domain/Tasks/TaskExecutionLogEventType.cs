namespace QPhising.Domain.Tasks;

public enum TaskExecutionLogEventType
{
    Claimed = 1,
    Started = 2,
    Succeeded = 3,
    Failed = 4,
    Retried = 5,
    DeadLettered = 6
}
