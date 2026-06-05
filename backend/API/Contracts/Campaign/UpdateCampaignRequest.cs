namespace QPhising.Api.Contracts.Campaign;

public sealed record UpdateCampaignRequest(
    string Name,
    DateTimeOffset? StartsAtUtc = null,
    DateTimeOffset? EndsAtUtc = null);
