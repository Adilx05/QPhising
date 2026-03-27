namespace QPhising.Domain.Campaigns.Exceptions;

public sealed class CampaignValidationException : CampaignDomainException
{
    public CampaignValidationException(string message)
        : base(message)
    {
    }
}
