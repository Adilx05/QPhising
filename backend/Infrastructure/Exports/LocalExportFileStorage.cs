using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using QPhising.Application.Common.Abstractions.Exports;

namespace QPhising.Infrastructure.Exports;

public sealed class LocalExportFileStorage(IOptions<ExportStorageOptions> options) : IExportFileStorage
{
    private static readonly Regex InvalidFileCharactersRegex = new("[^a-zA-Z0-9._-]", RegexOptions.Compiled);
    private readonly ExportStorageOptions _options = options.Value;

    public async Task<StoredExportFile> SaveAsync(
        Guid exportJobId,
        ExportBinaryFile file,
        CancellationToken cancellationToken = default)
    {
        if (file.Content.Length == 0)
        {
            throw new InvalidOperationException("Export file content must not be empty.");
        }

        string normalizedBasePath = Path.GetFullPath(_options.BasePath);
        string exportDirectory = Path.Combine(
            normalizedBasePath,
            DateTimeOffset.UtcNow.ToString("yyyy"),
            DateTimeOffset.UtcNow.ToString("MM"),
            DateTimeOffset.UtcNow.ToString("dd"));

        Directory.CreateDirectory(exportDirectory);

        string sanitizedName = SanitizeFileName(file.FileName);
        string resolvedFileName = $"{exportJobId:N}_{sanitizedName}";
        string fullPath = Path.Combine(exportDirectory, resolvedFileName);

        await File.WriteAllBytesAsync(fullPath, file.Content, cancellationToken);

        return new StoredExportFile(
            StoragePath: fullPath,
            SizeBytes: file.Content.LongLength);
    }

    public async Task<ExportFileContent?> TryReadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
        {
            return null;
        }

        string normalizedBasePath = Path.GetFullPath(_options.BasePath);
        string normalizedStoragePath = Path.GetFullPath(storagePath);

        if (!normalizedStoragePath.StartsWith(normalizedBasePath, StringComparison.Ordinal))
        {
            return null;
        }

        if (!File.Exists(normalizedStoragePath))
        {
            return null;
        }

        byte[] content = await File.ReadAllBytesAsync(normalizedStoragePath, cancellationToken);
        return new ExportFileContent(content, content.LongLength);
    }

    public Task<bool> DeleteIfExistsAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
        {
            return Task.FromResult(false);
        }

        string normalizedBasePath = Path.GetFullPath(_options.BasePath);
        string normalizedStoragePath = Path.GetFullPath(storagePath);

        if (!normalizedStoragePath.StartsWith(normalizedBasePath, StringComparison.Ordinal))
        {
            return Task.FromResult(false);
        }

        if (!File.Exists(normalizedStoragePath))
        {
            return Task.FromResult(false);
        }

        File.Delete(normalizedStoragePath);
        return Task.FromResult(true);
    }

    private static string SanitizeFileName(string fileName)
    {
        string candidate = string.IsNullOrWhiteSpace(fileName) ? "export.bin" : fileName.Trim();
        candidate = InvalidFileCharactersRegex.Replace(candidate, "_");
        return candidate.Length == 0 ? "export.bin" : candidate;
    }
}
