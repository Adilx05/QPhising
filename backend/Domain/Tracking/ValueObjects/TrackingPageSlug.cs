using System.Text.RegularExpressions;

using QPhising.Domain.Common;

namespace QPhising.Domain.Tracking.ValueObjects;

public sealed partial class TrackingPageSlug : ValueObject
{
    public const int MaxLength = 80;

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled)]
    private static partial Regex SlugPattern();

    public TrackingPageSlug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Tracking page slug is required.", nameof(value));
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > MaxLength)
        {
            throw new ArgumentException($"Tracking page slug cannot exceed {MaxLength} characters.", nameof(value));
        }

        if (!SlugPattern().IsMatch(normalized))
        {
            throw new ArgumentException("Tracking page slug can only contain lowercase letters, numbers, and single dashes.", nameof(value));
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
