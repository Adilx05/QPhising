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
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        IQueuedTaskDispatcher dispatcher = scope.ServiceProvider.GetRequiredService<IQueuedTaskDispatcher>();

        QueuedTask? queuedTask = await taskRepository.ClaimNextAsync(
            TimeSpan.FromSeconds(_options.ClaimLeaseDurationSeconds),
            cancellationToken: cancellationToken);

        if (queuedTask is null)
        {
            return false;
        }

        logger.LogInformation(
            "Claimed task {TaskId}. Type={TaskType} Attempt={AttemptCount}/{MaxAttempts} CorrelationId={CorrelationId}",
            queuedTask.Id,
            queuedTask.Type,
            queuedTask.AttemptCount + 1,
            queuedTask.MaxAttempts,
            queuedTask.CorrelationId ?? "n/a");

        queuedTask.StartExecution();
        taskRepository.Update(queuedTask);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        QueuedTaskHandlerResult executionResult = await dispatcher.DispatchAsync(queuedTask, cancellationToken);

        if (executionResult.IsSuccess)
        {
            queuedTask.Complete();
            logger.LogInformation("Task {TaskId} completed successfully.", queuedTask.Id);
        }
        else
        {
            HandleTaskFailure(queuedTask, executionResult);
        }

        taskRepository.Update(queuedTask);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    private void HandleTaskFailure(QueuedTask queuedTask, QueuedTaskHandlerResult executionResult)
    {
        string failureMessage = executionResult.ErrorMessage ?? "Task execution failed.";
        DateTimeOffset failedAt = DateTimeOffset.UtcNow;

        queuedTask.Fail(failureMessage, failedAt);

        bool exhaustedAttempts = queuedTask.AttemptCount >= queuedTask.MaxAttempts;
        if (!executionResult.IsRetryable || exhaustedAttempts)
        {
            queuedTask.MoveToDeadLetter(failureMessage);
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

        logger.LogWarning(
            "Task {TaskId} failed and scheduled for retry at {NextAttemptAt}. Attempt={AttemptCount}/{MaxAttempts} DelaySeconds={DelaySeconds} Error={ErrorMessage}",
            queuedTask.Id,
            nextAttemptAt,
            queuedTask.AttemptCount,
            queuedTask.MaxAttempts,
            retryDelay.TotalSeconds,
            failureMessage);
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
