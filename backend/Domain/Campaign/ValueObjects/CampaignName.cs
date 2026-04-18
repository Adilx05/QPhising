using System;
using System.Collections.Generic;

using QPhising.Domain.Common;

namespace QPhising.Domain.Campaign.ValueObjects;

public sealed class CampaignName : ValueObject
{
    public const int MaxLength = 120;

    public CampaignName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Campaign name is required.", nameof(value));
        }

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
        {
            throw new ArgumentException($"Campaign name must be at most {MaxLength} characters.", nameof(value));
        }

        Value = normalized;
    }

    public string Value { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
