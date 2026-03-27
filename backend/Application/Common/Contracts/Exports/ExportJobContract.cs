using QPhising.Domain.Exports;

namespace QPhising.Application.Common.Contracts.Exports;

public sealed record ExportJobContract(
    Guid Id,
    string OwnerUserId,
    ExportType ExportType,
    ExportFormat Format,
    ExportJobStatus Status,
    DateTimeOffset RequestedAt,
    DateTimeOffset? QueuedAt,
    DateTimeOffset? ProcessingStartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? FailedAt,
    DateTimeOffset? CanceledAt,
    DateTimeOffset? ExpiresAt,
    string? FileName,
    string? StoragePath,
    string? ContentType,
    long? FileSizeBytes,
    string? ErrorMessage,
    string? CorrelationId);
