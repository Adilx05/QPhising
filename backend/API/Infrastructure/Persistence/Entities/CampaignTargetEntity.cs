namespace QPhising.Api.Infrastructure.Persistence.Entities;

public sealed class CampaignTargetEntity
{
    public Guid Id { get; set; }

    public Guid CampaignId { get; set; }

    public string EmailAddress { get; set; } = string.Empty;
}
