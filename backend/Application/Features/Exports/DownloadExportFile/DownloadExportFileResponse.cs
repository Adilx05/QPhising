namespace QPhising.Application.Features.Exports.DownloadExportFile;

public sealed record DownloadExportFileResponse(
    string FileName,
    string ContentType,
    byte[] Content);
