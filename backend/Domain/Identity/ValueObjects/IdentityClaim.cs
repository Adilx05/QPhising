using System;

namespace QPhising.Domain.Identity.ValueObjects;

public sealed class IdentityClaim : IEquatable<IdentityClaim>
{
    public IdentityClaim(string type, string value)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Identity claim type is required.", nameof(type));
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Identity claim value is required.", nameof(value));
        }

        Type = type.Trim();
        Value = value.Trim();
    }

    public string Type { get; }

    public string Value { get; }

    public bool Equals(IdentityClaim? other) =>
        other is not null &&
        string.Equals(Type, other.Type, StringComparison.OrdinalIgnoreCase) &&
        string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => obj is IdentityClaim other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(
        StringComparer.OrdinalIgnoreCase.GetHashCode(Type),
        StringComparer.OrdinalIgnoreCase.GetHashCode(Value));

    public override string ToString() => $"{Type}:{Value}";
}
