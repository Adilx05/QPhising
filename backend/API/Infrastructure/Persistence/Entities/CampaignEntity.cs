namespace QPhising.Api.Infrastructure.Persistence.Entities;

public sealed class CampaignEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid TemplateId { get; set; }

    public int LifecycleState { get; set; }

    public DateTimeOffset? ScheduleStartsAtUtc { get; set; }

    public DateTimeOffset? ScheduleEndsAtUtc { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public List<CampaignTargetEntity> Targets { get; set; } = [];
}
