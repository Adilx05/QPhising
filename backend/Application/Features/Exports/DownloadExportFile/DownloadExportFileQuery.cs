using MediatR;
using QPhising.Application.Common;

namespace QPhising.Application.Features.Exports.DownloadExportFile;

public sealed record DownloadExportFileQuery(
    Guid ExportJobId,
    string RequestingUserId,
    bool IsAdmin) : IRequest<Result<DownloadExportFileResponse>>;
