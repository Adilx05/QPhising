namespace QPhising.Api.Infrastructure.Persistence.Entities;

public sealed class TrackingPageEntity
{
    public Guid Id { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string DestinationUrl { get; set; } = string.Empty;

    public string OwnerId { get; set; } = string.Empty;

    public Guid? TemplateId { get; set; }

    public int PublishState { get; set; }

    public int? RetentionDays { get; set; }

    public bool? MaskIpAddress { get; set; }

    public bool? EnableBotFiltering { get; set; }

    public bool? CaptureUtmParameters { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public ICollection<VisitEventEntity> VisitEvents { get; set; } = new List<VisitEventEntity>();
}
