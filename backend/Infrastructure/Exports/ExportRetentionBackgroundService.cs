using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QPhising.Application.Common.Abstractions.Exports;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Exports;

namespace QPhising.Infrastructure.Exports;

public sealed class ExportRetentionBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<ExportRetentionOptions> retentionOptions,
    ILogger<ExportRetentionBackgroundService> logger) : BackgroundService
{
    private readonly ExportRetentionOptions _retentionOptions = retentionOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Export retention scheduler started. Interval={CleanupIntervalMinutes} minutes, BatchSize={CleanupBatchSize}.",
            _retentionOptions.CleanupIntervalMinutes,
            _retentionOptions.CleanupBatchSize);

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
                logger.LogError(exception, "Export retention cleanup cycle failed.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ExecuteCleanupCycleAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var exportJobRepository = scope.ServiceProvider.GetRequiredService<IExportJobRepository>();
        var exportFileStorage = scope.ServiceProvider.GetRequiredService<IExportFileStorage>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        DateTimeOffset asOfUtc = DateTimeOffset.UtcNow;
        int deletedFiles = 0;
        int touchedJobs = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            IReadOnlyCollection<ExportJob> expiredJobs = await exportJobRepository.ListExpiredWithStoredFileAsync(
                asOfUtc,
                _retentionOptions.CleanupBatchSize,
                cancellationToken);

            if (expiredJobs.Count == 0)
            {
                break;
            }

            foreach (ExportJob exportJob in expiredJobs)
            {
                if (string.IsNullOrWhiteSpace(exportJob.StoragePath))
                {
                    continue;
                }

                bool deleted = await exportFileStorage.DeleteIfExistsAsync(exportJob.StoragePath, cancellationToken);
                if (deleted)
                {
                    deletedFiles++;
                }

                exportJob.PurgeFileArtifact();
                exportJobRepository.Update(exportJob);
                touchedJobs++;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            if (expiredJobs.Count < _retentionOptions.CleanupBatchSize)
            {
                break;
            }
        }

        logger.LogInformation(
            "Export retention cleanup completed. AsOfUtc={AsOfUtc}, TouchedJobs={TouchedJobs}, DeletedFiles={DeletedFiles}.",
            asOfUtc,
            touchedJobs,
            deletedFiles);
    }
}
