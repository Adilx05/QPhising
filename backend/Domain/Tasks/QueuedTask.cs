using QPhising.Domain.Tasks.Exceptions;

namespace QPhising.Domain.Tasks;

public sealed class QueuedTask
{
    private static readonly IReadOnlyDictionary<TaskExecutionStatus, TaskExecutionStatus[]> AllowedTransitions =
        new Dictionary<TaskExecutionStatus, TaskExecutionStatus[]>
        {
            [TaskExecutionStatus.Queued] = [TaskExecutionStatus.Claimed, TaskExecutionStatus.Canceled],
            [TaskExecutionStatus.Claimed] = [TaskExecutionStatus.Running, TaskExecutionStatus.Failed, TaskExecutionStatus.Canceled],
            [TaskExecutionStatus.Running] = [TaskExecutionStatus.Succeeded, TaskExecutionStatus.Failed],
            [TaskExecutionStatus.Failed] = [TaskExecutionStatus.Queued, TaskExecutionStatus.DeadLettered],
            [TaskExecutionStatus.Succeeded] = [],
            [TaskExecutionStatus.DeadLettered] = [],
            [TaskExecutionStatus.Canceled] = []
        };

    private static readonly IReadOnlyDictionary<TaskType, string[]> RequiredPayloadKeysByType =
        new Dictionary<TaskType, string[]>
        {
            [TaskType.TrackingLinkGeneration] = ["campaignId", "recipientEmail"],
            [TaskType.TrackingClickProcessing] = ["campaignId", "trackingCode", "clickedAt"],
            [TaskType.ExportGeneration] = ["exportType", "requestedByUserId", "requestedAt"],
            [TaskType.CampaignActivation] = ["campaignId", "activateAt"]
        };

    private QueuedTask(
        Guid id,
        TaskType type,
        TaskPayload payload,
        int maxAttempts,
        DateTimeOffset createdAt,
        string? correlationId)
    {
        Id = id;
        Type = type;
        Payload = payload;
        MaxAttempts = maxAttempts;
        CreatedAt = createdAt;
        CorrelationId = correlationId;
        Status = TaskExecutionStatus.Queued;
        AttemptCount = 0;
    }

    public Guid Id { get; }

    public TaskType Type { get; }

    public TaskPayload Payload { get; }

    public TaskExecutionStatus Status { get; private set; }

    public int AttemptCount { get; private set; }

    public int MaxAttempts { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? ClaimedAt { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset? LastFailedAt { get; private set; }

    public DateTimeOffset? LeaseExpiresAt { get; private set; }

    public string? LastError { get; private set; }

    public string? CorrelationId { get; }

    public static QueuedTask Create(
        TaskType type,
        IReadOnlyDictionary<string, string> payloadValues,
        int maxAttempts,
        DateTimeOffset? createdAt = null,
        string? correlationId = null,
        Guid? id = null)
    {
        if (maxAttempts <= 0)
        {
            throw new TaskValidationException("Task max attempts must be greater than zero.");
        }

        TaskPayload payload = TaskPayload.Create(payloadValues);
        EnsurePayloadSchema(type, payload);

        string? normalizedCorrelationId = string.IsNullOrWhiteSpace(correlationId)
            ? null
            : correlationId.Trim();

        return new QueuedTask(
            id ?? Guid.NewGuid(),
            type,
            payload,
            maxAttempts,
            createdAt ?? DateTimeOffset.UtcNow,
            normalizedCorrelationId);
    }

    public void Claim(DateTimeOffset leaseExpiresAt, DateTimeOffset? claimedAt = null)
    {
        if (leaseExpiresAt <= (claimedAt ?? DateTimeOffset.UtcNow))
        {
            throw new TaskValidationException("Task lease expiration must be in the future.");
        }

        TransitionTo(TaskExecutionStatus.Claimed);

        ClaimedAt = claimedAt ?? DateTimeOffset.UtcNow;
        LeaseExpiresAt = leaseExpiresAt;
    }

    public void StartExecution(DateTimeOffset? startedAt = null)
    {
        TransitionTo(TaskExecutionStatus.Running);
        StartedAt = startedAt ?? DateTimeOffset.UtcNow;
        AttemptCount++;
        LastError = null;
    }

    public void Complete(DateTimeOffset? completedAt = null)
    {
        TransitionTo(TaskExecutionStatus.Succeeded);
        CompletedAt = completedAt ?? DateTimeOffset.UtcNow;
        LeaseExpiresAt = null;
        LastError = null;
    }

    public void Fail(string errorMessage, DateTimeOffset? failedAt = null)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new TaskValidationException("Task failure requires a non-empty error message.");
        }

        TransitionTo(TaskExecutionStatus.Failed);

        LastFailedAt = failedAt ?? DateTimeOffset.UtcNow;
        LeaseExpiresAt = null;
        LastError = errorMessage.Trim();
    }

    public void Requeue()
    {
        if (Status != TaskExecutionStatus.Failed)
        {
            throw new InvalidTaskStatusTransitionException(Status, TaskExecutionStatus.Queued);
        }

        if (AttemptCount >= MaxAttempts)
        {
            throw new TaskValidationException("Task cannot be requeued because it has exhausted max attempts.");
        }

        TransitionTo(TaskExecutionStatus.Queued);
        ClaimedAt = null;
        StartedAt = null;
    }

    public void MoveToDeadLetter(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new TaskValidationException("Dead-letter reason is required.");
        }

        if (Status != TaskExecutionStatus.Failed)
        {
            throw new InvalidTaskStatusTransitionException(Status, TaskExecutionStatus.DeadLettered);
        }

        TransitionTo(TaskExecutionStatus.DeadLettered);
        LastError = reason.Trim();
    }

    public void Cancel()
    {
        TransitionTo(TaskExecutionStatus.Canceled);
        LeaseExpiresAt = null;
    }

    private void TransitionTo(TaskExecutionStatus nextStatus)
    {
        if (Status == nextStatus)
        {
            throw new InvalidTaskStatusTransitionException(Status, nextStatus);
        }

        if (!AllowedTransitions.TryGetValue(Status, out TaskExecutionStatus[]? allowedNextStatuses) ||
            !allowedNextStatuses.Contains(nextStatus))
        {
            throw new InvalidTaskStatusTransitionException(Status, nextStatus);
        }

        Status = nextStatus;
    }

    private static void EnsurePayloadSchema(TaskType type, TaskPayload payload)
    {
        if (!RequiredPayloadKeysByType.TryGetValue(type, out string[]? requiredKeys))
        {
            throw new TaskValidationException($"Task type '{type}' payload schema is not configured.");
        }

        foreach (string key in requiredKeys)
        {
            payload.GetRequired(key);
        }
    }
}
