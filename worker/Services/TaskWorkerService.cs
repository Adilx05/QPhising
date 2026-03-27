using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Options;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Tasks;
using QPhising.Worker.Configuration;
using QPhising.Worker.TaskExecution;

namespace QPhising.Worker.Services;

public sealed class TaskWorkerService(
    IServiceScopeFactory scopeFactory,
    IOptions<TaskWorkerOptions> options,
    ILogger<TaskWorkerService> logger) : BackgroundService
{
    private readonly TaskWorkerOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Task worker service started at {TimestampUtc}. PollInterval={PollIntervalSeconds}s LeaseDuration={LeaseDurationSeconds}s",
            DateTimeOffset.UtcNow,
            _options.PollIntervalSeconds,
            _options.ClaimLeaseDurationSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                bool taskProcessed = await ProcessOneTaskAsync(stoppingToken);

                if (!taskProcessed)
                {
                    await Task.Delay(TimeSpan.FromSeconds(_options.PollIntervalSeconds), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Task worker loop failed unexpectedly. Retrying after delay.");
                await Task.Delay(TimeSpan.FromSeconds(_options.PollIntervalSeconds), stoppingToken);
            }
        }
    }

    private async Task<bool> ProcessOneTaskAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();

        IQueuedTaskRepository taskRepository = scope.ServiceProvider.GetRequiredService<IQueuedTaskRepository>();
        ITaskExecutionLogRepository taskExecutionLogRepository = scope.ServiceProvider.GetRequiredService<ITaskExecutionLogRepository>();
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        IQueuedTaskDispatcher dispatcher = scope.ServiceProvider.GetRequiredService<IQueuedTaskDispatcher>();

        QueuedTask? queuedTask = await taskRepository.ClaimNextAsync(
            TimeSpan.FromSeconds(_options.ClaimLeaseDurationSeconds),
            cancellationToken: cancellationToken);

        if (queuedTask is null)
        {
            return false;
        }

        await PersistExecutionLogAsync(
            taskExecutionLogRepository,
            unitOfWork,
            queuedTask,
            TaskExecutionLogEventType.Claimed,
            $"Task claimed with lease duration of {_options.ClaimLeaseDurationSeconds} second(s).",
            cancellationToken);

        logger.LogInformation(
            "Claimed task {TaskId}. Type={TaskType} Attempt={AttemptCount}/{MaxAttempts} CorrelationId={CorrelationId}",
            queuedTask.Id,
            queuedTask.Type,
            queuedTask.AttemptCount + 1,
            queuedTask.MaxAttempts,
            queuedTask.CorrelationId ?? "n/a");

        queuedTask.StartExecution();
        taskRepository.Update(queuedTask);

        await PersistExecutionLogAsync(
            taskExecutionLogRepository,
            unitOfWork,
            queuedTask,
            TaskExecutionLogEventType.Started,
            "Task execution started.",
            cancellationToken);

        Stopwatch stopwatch = Stopwatch.StartNew();
        QueuedTaskHandlerResult executionResult = await dispatcher.DispatchAsync(queuedTask, cancellationToken);
        stopwatch.Stop();

        if (executionResult.IsSuccess)
        {
            queuedTask.Complete();
            await PersistExecutionLogAsync(
                taskExecutionLogRepository,
                unitOfWork,
                queuedTask,
                TaskExecutionLogEventType.Succeeded,
                "Task execution completed successfully.",
                cancellationToken,
                stopwatch.ElapsedMilliseconds);

            logger.LogInformation(
                "Task {TaskId} completed successfully. DurationMs={ExecutionDurationMilliseconds}",
                queuedTask.Id,
                stopwatch.ElapsedMilliseconds);
        }
        else
        {
            await HandleTaskFailureAsync(
                queuedTask,
                executionResult,
                stopwatch.ElapsedMilliseconds,
                taskExecutionLogRepository,
                unitOfWork,
                cancellationToken);
        }

        taskRepository.Update(queuedTask);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task HandleTaskFailureAsync(
        QueuedTask queuedTask,
        QueuedTaskHandlerResult executionResult,
        long durationMilliseconds,
        ITaskExecutionLogRepository taskExecutionLogRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        string failureMessage = executionResult.ErrorMessage ?? "Task execution failed.";
        DateTimeOffset failedAt = DateTimeOffset.UtcNow;

        queuedTask.Fail(failureMessage, failedAt);

        await PersistExecutionLogAsync(
            taskExecutionLogRepository,
            unitOfWork,
            queuedTask,
            TaskExecutionLogEventType.Failed,
            failureMessage,
            cancellationToken,
            durationMilliseconds,
            executionResult.IsRetryable);

        bool exhaustedAttempts = queuedTask.AttemptCount >= queuedTask.MaxAttempts;
        if (!executionResult.IsRetryable || exhaustedAttempts)
        {
            queuedTask.MoveToDeadLetter(failureMessage);

            await PersistExecutionLogAsync(
                taskExecutionLogRepository,
                unitOfWork,
                queuedTask,
                TaskExecutionLogEventType.DeadLettered,
                "Task moved to dead-letter.",
                cancellationToken,
                durationMilliseconds,
                executionResult.IsRetryable,
                failureMessage);

            logger.LogWarning(
                "Task {TaskId} moved to dead-letter. Retryable={IsRetryable}. Attempt={AttemptCount}/{MaxAttempts}. Error={ErrorMessage}",
                queuedTask.Id,
                executionResult.IsRetryable,
                queuedTask.AttemptCount,
                queuedTask.MaxAttempts,
                failureMessage);

            return;
        }

        TimeSpan retryDelay = CalculateRetryDelay(queuedTask.AttemptCount);
        DateTimeOffset nextAttemptAt = failedAt.Add(retryDelay);
        queuedTask.RequeueForRetry(nextAttemptAt, failedAt);

        await PersistExecutionLogAsync(
            taskExecutionLogRepository,
            unitOfWork,
            queuedTask,
            TaskExecutionLogEventType.Retried,
            $"Task scheduled for retry at {nextAttemptAt:O}.",
            cancellationToken,
            durationMilliseconds,
            executionResult.IsRetryable,
            failureMessage,
            nextAttemptAt);

        logger.LogWarning(
            "Task {TaskId} failed and scheduled for retry at {NextAttemptAt}. Attempt={AttemptCount}/{MaxAttempts} DelaySeconds={DelaySeconds} Error={ErrorMessage}",
            queuedTask.Id,
            nextAttemptAt,
            queuedTask.AttemptCount,
            queuedTask.MaxAttempts,
            retryDelay.TotalSeconds,
            failureMessage);
    }

    private static async Task PersistExecutionLogAsync(
        ITaskExecutionLogRepository taskExecutionLogRepository,
        IUnitOfWork unitOfWork,
        QueuedTask queuedTask,
        TaskExecutionLogEventType eventType,
        string summary,
        CancellationToken cancellationToken,
        long? executionDurationMilliseconds = null,
        bool? isRetryable = null,
        string? errorMessage = null,
        DateTimeOffset? nextAttemptAt = null)
    {
        var detailsPayload = new Dictionary<string, object?>
        {
            ["summary"] = summary,
            ["taskType"] = queuedTask.Type.ToString(),
            ["status"] = queuedTask.Status.ToString(),
            ["attempt"] = queuedTask.AttemptCount,
            ["maxAttempts"] = queuedTask.MaxAttempts,
            ["traceId"] = Activity.Current?.TraceId.ToString(),
            ["retryable"] = isRetryable,
            ["error"] = errorMessage,
            ["nextAttemptAt"] = nextAttemptAt
        };

        string details = JsonSerializer.Serialize(detailsPayload);

        TaskExecutionLog executionLog = TaskExecutionLog.Create(
            taskId: queuedTask.Id,
            eventType: eventType,
            taskStatus: queuedTask.Status,
            attemptNumber: queuedTask.AttemptCount,
            correlationId: queuedTask.CorrelationId,
            details: details,
            executionDurationMilliseconds: executionDurationMilliseconds);

        await taskExecutionLogRepository.AddAsync(executionLog, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private TimeSpan CalculateRetryDelay(int attemptCount)
    {
        int boundedAttempt = Math.Max(attemptCount, 1);
        double exponentialDelay = _options.InitialRetryDelaySeconds *
                                  Math.Pow(_options.RetryBackoffMultiplier, boundedAttempt - 1);

        double cappedDelay = Math.Min(exponentialDelay, _options.MaxRetryDelaySeconds);
        return TimeSpan.FromSeconds(cappedDelay);
    }
}
