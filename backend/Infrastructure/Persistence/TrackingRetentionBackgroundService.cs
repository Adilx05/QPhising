using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QPhising.Domain.Abstractions;

namespace QPhising.Infrastructure.Persistence;

public sealed class TrackingRetentionBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<TrackingRetentionOptions> retentionOptions,
    ILogger<TrackingRetentionBackgroundService> logger) : BackgroundService
{
    private readonly TrackingRetentionOptions _retentionOptions = retentionOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Tracking retention scheduler started. Raw click retention {RawClickRetentionDays} days, aggregate retention {AggregateRetentionDays} days, interval {CleanupIntervalMinutes} minutes.",
            _retentionOptions.RawClickRetentionDays,
            _retentionOptions.AggregateRetentionDays,
            _retentionOptions.CleanupIntervalMinutes);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_retentionOptions.CleanupIntervalMinutes));

        do
        {
            try
            {
                await ExecuteCleanupCycleAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Tracking retention cleanup cycle failed.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ExecuteCleanupCycleAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var trackingClickRepository = scope.ServiceProvider.GetRequiredService<ITrackingClickRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        DateTimeOffset cutoffUtc = DateTimeOffset.UtcNow.AddDays(-_retentionOptions.RawClickRetentionDays);
        int deletedInCycle = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            int deleted = await trackingClickRepository.DeleteOlderThanAsync(
                cutoffUtc,
                _retentionOptions.CleanupBatchSize,
                cancellationToken);

            if (deleted == 0)
            {
                break;
            }

            deletedInCycle += deleted;
            await unitOfWork.SaveChangesAsync(cancellationToken);

            if (deleted < _retentionOptions.CleanupBatchSize)
            {
                break;
            }
        }

        logger.LogInformation(
            "Tracking retention cleanup completed. CutoffUtc={CutoffUtc}, DeletedRawClicks={DeletedRawClicks}.",
            cutoffUtc,
            deletedInCycle);
    }
}
