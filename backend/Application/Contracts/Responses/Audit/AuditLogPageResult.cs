namespace QPhising.Application.Contracts.Responses.Audit;

public sealed record AuditLogPageResult(
    IReadOnlyCollection<AuditLogEntryResult> Items,
    int Page,
    int PageSize,
    int TotalCount);
