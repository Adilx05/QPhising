using Microsoft.Extensions.Options;
using QPhising.Application.Common.Abstractions;
using QPhising.Application.Common.Abstractions.Exports;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Campaigns;
using QPhising.Domain.Exports;
using QPhising.Domain.Tasks;
using QPhising.Infrastructure.Exports;

namespace QPhising.Worker.TaskExecution.Handlers;

public sealed class ExportGenerationTaskHandler(
    IExportJobRepository exportJobRepository,
    ICampaignRepository campaignRepository,
    IAnalyticsReadRepository analyticsReadRepository,
    IExcelExportService excelExportService,
    IPdfExportService pdfExportService,
    IExportFileStorage exportFileStorage,
    IUnitOfWork unitOfWork,
    IOptions<ExportStorageOptions> exportStorageOptions,
    ILogger<ExportGenerationTaskHandler> logger) : IQueuedTaskHandler
{
    private readonly ExportStorageOptions _exportStorageOptions = exportStorageOptions.Value;

    public TaskType TaskType => TaskType.ExportGeneration;

    public async Task<QueuedTaskHandlerResult> HandleAsync(QueuedTask queuedTask, CancellationToken cancellationToken)
    {
        string exportJobIdValue = queuedTask.Payload.GetRequired("exportJobId");
        if (!Guid.TryParse(exportJobIdValue, out Guid exportJobId))
        {
            return QueuedTaskHandlerResult.Failure(
                $"Task payload exportJobId '{exportJobIdValue}' is invalid.",
                isRetryable: false);
        }

        ExportJob? exportJob = await exportJobRepository.GetByIdAsync(exportJobId, cancellationToken);
        if (exportJob is null)
        {
            return QueuedTaskHandlerResult.Failure(
                $"Export job '{exportJobId}' was not found.",
                isRetryable: false);
        }

        try
        {
            if (!TryMoveToProcessing(exportJob, out string? transitionError))
            {
                return QueuedTaskHandlerResult.Failure(transitionError!, isRetryable: false);
            }

            exportJobRepository.Update(exportJob);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            ExportBinaryFile exportFile = await BuildExportBinaryAsync(exportJob, cancellationToken);
            StoredExportFile storedFile = await exportFileStorage.SaveAsync(exportJob.Id, exportFile, cancellationToken);

            DateTimeOffset completedAt = DateTimeOffset.UtcNow;
            exportJob.Complete(
                fileName: exportFile.FileName,
                storagePath: storedFile.StoragePath,
                contentType: exportFile.ContentType,
                fileSizeBytes: storedFile.SizeBytes,
                expiresAt: completedAt.AddDays(_exportStorageOptions.FileTtlDays),
                completedAt: completedAt);

            exportJobRepository.Update(exportJob);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return QueuedTaskHandlerResult.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Export generation failed for ExportJobId={ExportJobId}", exportJob.Id);

            if (exportJob.Status == ExportJobStatus.Processing)
            {
                exportJob.Fail(exception.Message);
                exportJobRepository.Update(exportJob);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return QueuedTaskHandlerResult.Failure(exception.Message, isRetryable: true);
        }
    }

    private async Task<ExportBinaryFile> BuildExportBinaryAsync(ExportJob exportJob, CancellationToken cancellationToken)
    {
        return (exportJob.ExportType, exportJob.Format) switch
        {
            (ExportType.CampaignReport, ExportFormat.Excel) => await BuildCampaignExcelAsync(cancellationToken),
            (ExportType.CampaignReport, ExportFormat.Pdf) => await BuildCampaignPdfAsync(cancellationToken),
            (ExportType.AnalyticsReport, ExportFormat.Excel) => await BuildAnalyticsExcelAsync(cancellationToken),
            (ExportType.AnalyticsReport, ExportFormat.Pdf) => await BuildAnalyticsPdfAsync(cancellationToken),
            _ => throw new InvalidOperationException(
                $"Unsupported export combination: Type={exportJob.ExportType}, Format={exportJob.Format}.")
        };
    }

    private async Task<ExportBinaryFile> BuildCampaignExcelAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Campaign> campaigns = await campaignRepository.ListAsync(new CampaignReadCriteria(), cancellationToken);
        return await excelExportService.BuildCampaignReportAsync(campaigns, cancellationToken);
    }

    private async Task<ExportBinaryFile> BuildCampaignPdfAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Campaign> campaigns = await campaignRepository.ListAsync(new CampaignReadCriteria(), cancellationToken);
        return await pdfExportService.BuildCampaignReportAsync(campaigns, cancellationToken);
    }

    private async Task<ExportBinaryFile> BuildAnalyticsExcelAsync(CancellationToken cancellationToken)
    {
        AnalyticsReadCriteria criteria = BuildDefaultAnalyticsCriteria();
        AnalyticsDashboardReadModel dashboard = await analyticsReadRepository.GetDashboardReadModelAsync(criteria, cancellationToken);
        return await excelExportService.BuildAnalyticsReportAsync(dashboard, criteria, cancellationToken);
    }

    private async Task<ExportBinaryFile> BuildAnalyticsPdfAsync(CancellationToken cancellationToken)
    {
        AnalyticsReadCriteria criteria = BuildDefaultAnalyticsCriteria();
        AnalyticsDashboardReadModel dashboard = await analyticsReadRepository.GetDashboardReadModelAsync(criteria, cancellationToken);
        return await pdfExportService.BuildAnalyticsReportAsync(dashboard, criteria, cancellationToken);
    }

    private static AnalyticsReadCriteria BuildDefaultAnalyticsCriteria()
    {
        DateTimeOffset toUtc = DateTimeOffset.UtcNow;
        DateTimeOffset fromUtc = toUtc.AddDays(-30);

        return new AnalyticsReadCriteria(
            From: fromUtc,
            To: toUtc,
            CampaignIds: [],
            TemplateTypes: [],
            CampaignStatuses: []);
    }

    private static bool TryMoveToProcessing(ExportJob exportJob, out string? error)
    {
        error = null;

        switch (exportJob.Status)
        {
            case ExportJobStatus.Requested:
                exportJob.Queue();
                exportJob.StartProcessing();
                return true;
            case ExportJobStatus.Queued:
                exportJob.StartProcessing();
                return true;
            case ExportJobStatus.Failed:
                exportJob.Queue();
                exportJob.StartProcessing();
                return true;
            case ExportJobStatus.Processing:
                return true;
            default:
                error = $"Export job '{exportJob.Id}' status '{exportJob.Status}' cannot be processed.";
                return false;
        }
    }
}
