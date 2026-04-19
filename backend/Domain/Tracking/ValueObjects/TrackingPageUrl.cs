using QPhising.Domain.Common;

namespace QPhising.Domain.Tracking.ValueObjects;

public sealed class TrackingPageUrl : ValueObject
{
    public const int MaxLength = 2048;

    public TrackingPageUrl(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Tracking page URL is required.", nameof(value));
        }

        var normalized = value.Trim();
        if (normalized.Length > MaxLength)
        {
            throw new ArgumentException($"Tracking page URL cannot exceed {MaxLength} characters.", nameof(value));
        }

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var parsed))
        {
            throw new ArgumentException("Tracking page URL must be a valid absolute URL.", nameof(value));
        }

        if (parsed.Scheme is not ("https" or "http"))
        {
            throw new ArgumentException("Tracking page URL must use HTTP or HTTPS.", nameof(value));
        }

        Value = parsed.AbsoluteUri;
    }

    public string Value { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
