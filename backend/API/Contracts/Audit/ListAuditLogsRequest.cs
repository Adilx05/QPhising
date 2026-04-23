using QPhising.Application.Contracts.Abstractions.Audit;

namespace QPhising.Api.Contracts.Audit;

public sealed record ListAuditLogsRequest(
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    string? Actor,
    string? Endpoint,
    int? OutcomeCode,
    string? CorrelationId,
    int Page = 1,
    int PageSize = 25,
    AuditLogSortField SortBy = AuditLogSortField.Timestamp,
    SortDirection SortDirection = SortDirection.Desc);
