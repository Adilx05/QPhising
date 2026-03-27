namespace QPhising.Application.Common.Abstractions.Exports;

public interface IExportFileStorage
{
    Task<StoredExportFile> SaveAsync(
        Guid exportJobId,
        ExportBinaryFile file,
        CancellationToken cancellationToken = default);

    Task<ExportFileContent?> TryReadAsync(
        string storagePath,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteIfExistsAsync(
        string storagePath,
        CancellationToken cancellationToken = default);
}

public sealed record StoredExportFile(
    string StoragePath,
    long SizeBytes);

public sealed record ExportFileContent(
    byte[] Content,
    long SizeBytes);
