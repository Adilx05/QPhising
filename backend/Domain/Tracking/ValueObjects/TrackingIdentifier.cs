using System.Text.RegularExpressions;

using QPhising.Domain.Common;

namespace QPhising.Domain.Tracking.ValueObjects;

public sealed partial class TrackingIdentifier : ValueObject
{
    public const int MaxLength = 128;

    [GeneratedRegex("^[A-Za-z0-9_.:-]+$", RegexOptions.Compiled)]
    private static partial Regex IdentifierPattern();

    public TrackingIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Tracking identifier is required.", nameof(value));
        }

        var normalized = value.Trim();
        if (normalized.Length > MaxLength)
        {
            throw new ArgumentException($"Tracking identifier cannot exceed {MaxLength} characters.", nameof(value));
        }

        if (!IdentifierPattern().IsMatch(normalized))
        {
            throw new ArgumentException("Tracking identifier can only contain letters, numbers, underscore, dash, dot, and colon.", nameof(value));
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
