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
            queuedTask.Fail(executionResult.ErrorMessage ?? "Task execution failed.");
            logger.LogWarning(
                "Task {TaskId} failed. Retryable={IsRetryable}. Error={ErrorMessage}",
                queuedTask.Id,
                executionResult.IsRetryable,
                executionResult.ErrorMessage);
        }

        taskRepository.Update(queuedTask);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
