using System;
using System.Collections.Generic;

using QPhising.Domain.Common;

namespace QPhising.Domain.Campaign.ValueObjects;

public sealed class CampaignScheduleWindow : ValueObject
{
    public CampaignScheduleWindow(DateTimeOffset startsAtUtc, DateTimeOffset? endsAtUtc)
    {
        if (startsAtUtc == DateTimeOffset.MinValue)
        {
            throw new ArgumentException("Campaign start time is required.", nameof(startsAtUtc));
        }

        if (endsAtUtc.HasValue && endsAtUtc.Value <= startsAtUtc)
        {
            throw new ArgumentException("Campaign end time must be after the start time.", nameof(endsAtUtc));
        }

        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
    }

    public DateTimeOffset StartsAtUtc { get; }

    public DateTimeOffset? EndsAtUtc { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartsAtUtc;
        yield return EndsAtUtc;
    }
}
