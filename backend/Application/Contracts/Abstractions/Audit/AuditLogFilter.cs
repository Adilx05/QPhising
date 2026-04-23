namespace QPhising.Application.Contracts.Abstractions.Audit;

public sealed record AuditLogFilter(
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    string? Actor,
    string? Endpoint,
    int? OutcomeCode,
    string? CorrelationId,
    int Page,
    int PageSize,
    AuditLogSortField SortBy,
    SortDirection SortDirection);
