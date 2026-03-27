namespace QPhising.Worker.TaskExecution;

public sealed record QueuedTaskHandlerResult(bool IsSuccess, string? ErrorMessage = null, bool IsRetryable = true)
{
    public static QueuedTaskHandlerResult Success() => new(true);

    public static QueuedTaskHandlerResult Failure(string errorMessage, bool isRetryable = true)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentException("Error message is required.", nameof(errorMessage));
        }

        return new QueuedTaskHandlerResult(false, errorMessage.Trim(), isRetryable);
    }
}
