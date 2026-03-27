namespace QPhising.Domain.Campaigns.Events;

public sealed record CampaignStatusChangedDomainEvent(
    Guid CampaignId,
    CampaignStatus PreviousStatus,
    CampaignStatus CurrentStatus,
    DateTimeOffset OccurredAt);
