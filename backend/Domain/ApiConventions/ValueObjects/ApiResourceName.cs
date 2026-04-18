using System.Text.RegularExpressions;

namespace QPhising.Domain.ApiConventions.ValueObjects;

/// <summary>
/// Represents a normalized API resource segment name used in route definitions.
/// </summary>
public sealed partial class ApiResourceName : IEquatable<ApiResourceName>
{
    private static readonly Regex AllowedPattern = ResourceNameRegex();

    public string Value { get; }

    private ApiResourceName(string value)
    {
        Value = value;
    }

    public static ApiResourceName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Resource name is required.", nameof(value));
        }

        var normalized = value.Trim();
        if (!AllowedPattern.IsMatch(normalized))
        {
            throw new ArgumentException(
                "Resource name must be lowercase kebab-case and plural (for example: setup-configurations).",
                nameof(value));
        }

        return new ApiResourceName(normalized);
    }

    public bool Equals(ApiResourceName? other)
    {
        if (other is null)
        {
            return false;
        }

        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ApiResourceName);
    }

    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(Value);
    }

    public override string ToString()
    {
        return Value;
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*s$")]
    private static partial Regex ResourceNameRegex();
}
