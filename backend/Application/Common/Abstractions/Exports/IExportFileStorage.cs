namespace QPhising.Application.Common.Abstractions.Exports;

public interface IExportFileStorage
{
    Task<StoredExportFile> SaveAsync(
        Guid exportJobId,
        ExportBinaryFile file,
        CancellationToken cancellationToken = default);
}

public sealed record StoredExportFile(
    string StoragePath,
    long SizeBytes);
