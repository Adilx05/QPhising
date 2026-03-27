using QPhising.Domain.Exports;

namespace QPhising.Domain.Abstractions;

public sealed record ExportJobReadCriteria(
    string? OwnerUserId = null,
    IReadOnlyCollection<ExportJobStatus>? Statuses = null,
    IReadOnlyCollection<ExportType>? ExportTypes = null,
    IReadOnlyCollection<ExportFormat>? Formats = null,
    DateTimeOffset? RequestedFrom = null,
    DateTimeOffset? RequestedTo = null,
    int? Skip = null,
    int? Take = null);
