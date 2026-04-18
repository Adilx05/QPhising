using System;
using System.Net.Mail;

using QPhising.Domain.Common;

namespace QPhising.Domain.Campaign.Entities;

public sealed class CampaignTarget : Entity<Guid>
{
    public CampaignTarget(Guid id, string emailAddress)
        : base(id)
    {
        EmailAddress = NormalizeEmail(emailAddress);
    }

    public string EmailAddress { get; private set; }

    public void UpdateEmailAddress(string emailAddress)
    {
        EmailAddress = NormalizeEmail(emailAddress);
    }

    private static string NormalizeEmail(string emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress))
        {
            throw new ArgumentException("Target email address is required.", nameof(emailAddress));
        }

        try
        {
            var mailAddress = new MailAddress(emailAddress.Trim());
            return mailAddress.Address;
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Target email address is invalid.", nameof(emailAddress), ex);
        }
    }
}
