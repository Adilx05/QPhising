namespace QPhising.Api.Infrastructure.Persistence.Entities;

public sealed class TemplateEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Tags { get; set; } = string.Empty;

    public int LifecycleState { get; set; }

    public int Version { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
