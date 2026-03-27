using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions.Exports;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Exports;

namespace QPhising.Application.Features.Exports.DownloadExportFile;

public sealed class DownloadExportFileQueryHandler(
    IExportJobRepository exportJobRepository,
    IExportFileStorage exportFileStorage) : IRequestHandler<DownloadExportFileQuery, Result<DownloadExportFileResponse>>
{
    public async Task<Result<DownloadExportFileResponse>> Handle(DownloadExportFileQuery request, CancellationToken cancellationToken)
    {
        ExportJob? exportJob = await exportJobRepository.GetByIdAsync(request.ExportJobId, cancellationToken);
        if (exportJob is null)
        {
            return Result<DownloadExportFileResponse>.Failure($"Export job '{request.ExportJobId}' was not found.");
        }

        if (!request.IsAdmin && !string.Equals(exportJob.OwnerUserId, request.RequestingUserId, StringComparison.Ordinal))
        {
            return Result<DownloadExportFileResponse>.Failure("forbidden");
        }

        if (exportJob.Status != ExportJobStatus.Completed ||
            string.IsNullOrWhiteSpace(exportJob.StoragePath) ||
            string.IsNullOrWhiteSpace(exportJob.FileName) ||
            string.IsNullOrWhiteSpace(exportJob.ContentType))
        {
            return Result<DownloadExportFileResponse>.Failure("Export file is not ready for download.");
        }

        if (exportJob.ExpiresAt is { } expiresAt && expiresAt <= DateTimeOffset.UtcNow)
        {
            return Result<DownloadExportFileResponse>.Failure("Export file has expired.");
        }

        ExportFileContent? content = await exportFileStorage.TryReadAsync(exportJob.StoragePath, cancellationToken);
        if (content is null)
        {
            return Result<DownloadExportFileResponse>.Failure("Export file was not found in storage.");
        }

        return Result<DownloadExportFileResponse>.Success(new DownloadExportFileResponse(
            exportJob.FileName,
            exportJob.ContentType,
            content.Content));
    }
}
