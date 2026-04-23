using MediatR;
using QPhising.Application.Contracts.Abstractions.Audit;
using QPhising.Application.Contracts.Responses.Audit;

namespace QPhising.Application.CQRS.Queries.Audit;

public sealed record ListAuditLogsQuery(
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    string? Actor,
    string? Endpoint,
    int? OutcomeCode,
    string? CorrelationId,
    int Page = 1,
    int PageSize = 25,
    AuditLogSortField SortBy = AuditLogSortField.Timestamp,
    SortDirection SortDirection = SortDirection.Desc) : IRequest<AuditLogPageResult>;
