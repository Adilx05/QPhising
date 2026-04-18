namespace QPhising.Api.Contracts.Campaign;

public sealed record ScheduleCampaignRequest(
    DateTimeOffset StartsAtUtc,
    DateTimeOffset? EndsAtUtc);
