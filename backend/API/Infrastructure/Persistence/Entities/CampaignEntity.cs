namespace QPhising.Api.Infrastructure.Persistence.Entities;

public sealed class CampaignEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public Guid TrackingPageId { get; set; }

    public Guid? TemplateId { get; set; }

    public int LifecycleState { get; set; }

    public DateTimeOffset? ScheduleStartsAtUtc { get; set; }

    public DateTimeOffset? ScheduleEndsAtUtc { get; set; }

}
