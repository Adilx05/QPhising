namespace QPhising.Application.Contracts.Responses.Campaign;

public sealed record CampaignTargetResult(
    Guid Id,
    string EmailAddress);
