using QPhising.Domain.Exports.Exceptions;

namespace QPhising.Domain.Exports;

public sealed class ExportJob
{
    private const int MaxOwnerUserIdLength = 200;
    private const int MaxFileNameLength = 255;
    private const int MaxStoragePathLength = 1024;
    private const int MaxContentTypeLength = 150;
    private const int MaxCorrelationIdLength = 100;

    private static readonly IReadOnlyDictionary<ExportJobStatus, ExportJobStatus[]> AllowedTransitions =
        new Dictionary<ExportJobStatus, ExportJobStatus[]>
        {
            [ExportJobStatus.Requested] = [ExportJobStatus.Queued, ExportJobStatus.Canceled],
            [ExportJobStatus.Queued] = [ExportJobStatus.Processing, ExportJobStatus.Canceled],
            [ExportJobStatus.Processing] = [ExportJobStatus.Completed, ExportJobStatus.Failed, ExportJobStatus.Canceled],
            [ExportJobStatus.Completed] = [],
            [ExportJobStatus.Failed] = [ExportJobStatus.Queued, ExportJobStatus.Canceled],
            [ExportJobStatus.Canceled] = []
        };

    private ExportJob(
        Guid id,
        string ownerUserId,
        ExportType exportType,
        ExportFormat format,
        DateTimeOffset requestedAt,
        string? correlationId)
    {
        Id = id;
        OwnerUserId = ownerUserId;
        ExportType = exportType;
        Format = format;
        RequestedAt = requestedAt;
        CorrelationId = correlationId;
        Status = ExportJobStatus.Requested;
    }

    public Guid Id { get; }

    public string OwnerUserId { get; }

    public ExportType ExportType { get; }

    public ExportFormat Format { get; }

    public ExportJobStatus Status { get; private set; }

    public DateTimeOffset RequestedAt { get; }

    public DateTimeOffset? QueuedAt { get; private set; }

    public DateTimeOffset? ProcessingStartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset? FailedAt { get; private set; }

    public DateTimeOffset? CanceledAt { get; private set; }

    public DateTimeOffset? ExpiresAt { get; private set; }

    public string? FileName { get; private set; }

    public string? StoragePath { get; private set; }

    public string? ContentType { get; private set; }

    public long? FileSizeBytes { get; private set; }

    public string? ErrorMessage { get; private set; }

    public string? CorrelationId { get; }

    public static ExportJob Create(
        string ownerUserId,
        ExportType exportType,
        ExportFormat format,
        DateTimeOffset? requestedAt = null,
        string? correlationId = null,
        Guid? id = null)
    {
        string normalizedOwnerUserId = ValidateAndNormalizeOwnerUserId(ownerUserId);
        string? normalizedCorrelationId = NormalizeOptionalValue(
            correlationId,
            MaxCorrelationIdLength,
            "Export correlation ID");

        return new ExportJob(
            id ?? Guid.NewGuid(),
            normalizedOwnerUserId,
            exportType,
            format,
            requestedAt ?? DateTimeOffset.UtcNow,
            normalizedCorrelationId);
    }

    public void Queue(DateTimeOffset? queuedAt = null)
    {
        TransitionTo(ExportJobStatus.Queued);
        QueuedAt = queuedAt ?? DateTimeOffset.UtcNow;
        ErrorMessage = null;
    }

    public void StartProcessing(DateTimeOffset? startedAt = null)
    {
        TransitionTo(ExportJobStatus.Processing);
        ProcessingStartedAt = startedAt ?? DateTimeOffset.UtcNow;
        ErrorMessage = null;
    }

    public void Complete(
        string fileName,
        string storagePath,
        string contentType,
        long fileSizeBytes,
        DateTimeOffset expiresAt,
        DateTimeOffset? completedAt = null)
    {
        DateTimeOffset effectiveCompletedAt = completedAt ?? DateTimeOffset.UtcNow;
        if (expiresAt <= effectiveCompletedAt)
        {
            throw new ExportValidationException("Export file expiration must be later than completion timestamp.");
        }

        if (fileSizeBytes <= 0)
        {
            throw new ExportValidationException("Export file size must be greater than zero bytes.");
        }

        TransitionTo(ExportJobStatus.Completed);

        FileName = ValidateAndNormalizeRequiredValue(fileName, MaxFileNameLength, "Export file name");
        StoragePath = ValidateAndNormalizeRequiredValue(storagePath, MaxStoragePathLength, "Export storage path");
        ContentType = ValidateAndNormalizeRequiredValue(contentType, MaxContentTypeLength, "Export content type");
        FileSizeBytes = fileSizeBytes;
        CompletedAt = effectiveCompletedAt;
        ExpiresAt = expiresAt;
        FailedAt = null;
        ErrorMessage = null;
    }

    public void Fail(string errorMessage, DateTimeOffset? failedAt = null)
    {
        TransitionTo(ExportJobStatus.Failed);
        FailedAt = failedAt ?? DateTimeOffset.UtcNow;
        ErrorMessage = ValidateAndNormalizeRequiredValue(errorMessage, 2000, "Export failure message");
        CompletedAt = null;
        ExpiresAt = null;
        FileName = null;
        StoragePath = null;
        ContentType = null;
        FileSizeBytes = null;
    }

    public void Cancel(DateTimeOffset? canceledAt = null)
    {
        TransitionTo(ExportJobStatus.Canceled);
        CanceledAt = canceledAt ?? DateTimeOffset.UtcNow;
    }

    public void PurgeFileArtifact()
    {
        if (Status != ExportJobStatus.Completed)
        {
            throw new ExportValidationException("Only completed export jobs can purge file artifacts.");
        }

        FileName = null;
        StoragePath = null;
        ContentType = null;
        FileSizeBytes = null;
    }

    private void TransitionTo(ExportJobStatus requestedStatus)
    {
        if (Status == requestedStatus)
        {
            throw new InvalidExportJobStatusTransitionException(Status, requestedStatus);
        }

        if (!AllowedTransitions.TryGetValue(Status, out ExportJobStatus[]? allowedStatuses) ||
            !allowedStatuses.Contains(requestedStatus))
        {
            throw new InvalidExportJobStatusTransitionException(Status, requestedStatus);
        }

        Status = requestedStatus;
    }

    private static string ValidateAndNormalizeOwnerUserId(string ownerUserId) =>
        ValidateAndNormalizeRequiredValue(ownerUserId, MaxOwnerUserIdLength, "Export owner user ID");

    private static string ValidateAndNormalizeRequiredValue(string value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ExportValidationException($"{fieldName} is required.");
        }

        string normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ExportValidationException($"{fieldName} must be {maxLength} characters or fewer.");
        }

        return normalized;
    }

    private static string? NormalizeOptionalValue(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ExportValidationException($"{fieldName} must be {maxLength} characters or fewer.");
        }

        return normalized;
    }
}
