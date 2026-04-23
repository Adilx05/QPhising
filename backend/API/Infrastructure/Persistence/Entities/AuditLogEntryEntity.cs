namespace QPhising.Api.Infrastructure.Persistence.Entities;

public sealed class AuditLogEntryEntity
{
    public Guid Id { get; set; }

    public DateTimeOffset TimestampUtc { get; set; }

    public string Actor { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string Resource { get; set; } = string.Empty;

    public string Outcome { get; set; } = string.Empty;

    public int OutcomeCode { get; set; }

    public string CorrelationId { get; set; } = string.Empty;

    public string? IpHash { get; set; }
}
