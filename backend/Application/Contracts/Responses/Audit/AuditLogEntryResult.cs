namespace QPhising.Application.Contracts.Responses.Audit;

public sealed record AuditLogEntryResult(
    Guid Id,
    DateTimeOffset TimestampUtc,
    string Actor,
    string Action,
    string Resource,
    string Outcome,
    int OutcomeCode,
    string CorrelationId,
    string? IpHash);
