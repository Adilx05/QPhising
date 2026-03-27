namespace QPhising.Domain.Campaigns.Exceptions;

public class CampaignDomainException : Exception
{
    public CampaignDomainException(string message)
        : base(message)
    {
    }
}
