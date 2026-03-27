namespace QPhising.Domain.Campaigns.Exceptions;

public sealed class InvalidCampaignStatusTransitionException : CampaignDomainException
{
    public InvalidCampaignStatusTransitionException(CampaignStatus currentStatus, CampaignStatus requestedStatus)
        : base($"Invalid campaign status transition from '{currentStatus}' to '{requestedStatus}'.")
    {
        CurrentStatus = currentStatus;
        RequestedStatus = requestedStatus;
    }

    public CampaignStatus CurrentStatus { get; }

    public CampaignStatus RequestedStatus { get; }
}
