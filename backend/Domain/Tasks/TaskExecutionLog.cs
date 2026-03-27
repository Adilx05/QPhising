using QPhising.Domain.Tasks.Exceptions;

namespace QPhising.Domain.Tasks;

public sealed class TaskExecutionLog
{
    private TaskExecutionLog(
        Guid id,
        Guid taskId,
        TaskExecutionLogEventType eventType,
        TaskExecutionStatus taskStatus,
        int attemptNumber,
        DateTimeOffset occurredAt,
        string? correlationId,
        string? details,
        long? executionDurationMilliseconds)
    {
        Id = id;
        TaskId = taskId;
        EventType = eventType;
        TaskStatus = taskStatus;
        AttemptNumber = attemptNumber;
        OccurredAt = occurredAt;
        CorrelationId = correlationId;
        Details = details;
        ExecutionDurationMilliseconds = executionDurationMilliseconds;
    }

    public Guid Id { get; }

    public Guid TaskId { get; }

    public TaskExecutionLogEventType EventType { get; }

    public TaskExecutionStatus TaskStatus { get; }

    public int AttemptNumber { get; }

    public DateTimeOffset OccurredAt { get; }

    public string? CorrelationId { get; }

    public string? Details { get; }

    public long? ExecutionDurationMilliseconds { get; }

    public static TaskExecutionLog Create(
        Guid taskId,
        TaskExecutionLogEventType eventType,
        TaskExecutionStatus taskStatus,
        int attemptNumber,
        DateTimeOffset? occurredAt = null,
        string? correlationId = null,
        string? details = null,
        long? executionDurationMilliseconds = null,
        Guid? id = null)
    {
        if (taskId == Guid.Empty)
        {
            throw new TaskValidationException("Task execution log requires a non-empty task identifier.");
        }

        if (attemptNumber < 0)
        {
            throw new TaskValidationException("Task execution log attempt number must be zero or greater.");
        }

        if (executionDurationMilliseconds is < 0)
        {
            throw new TaskValidationException("Task execution duration cannot be negative.");
        }

        string? normalizedCorrelationId = string.IsNullOrWhiteSpace(correlationId)
            ? null
            : correlationId.Trim();

        string? normalizedDetails = string.IsNullOrWhiteSpace(details)
            ? null
            : details.Trim();

        return new TaskExecutionLog(
            id ?? Guid.NewGuid(),
            taskId,
            eventType,
            taskStatus,
            attemptNumber,
            occurredAt ?? DateTimeOffset.UtcNow,
            normalizedCorrelationId,
            normalizedDetails,
            executionDurationMilliseconds);
    }
}
