using System;

namespace QPhising.Domain.Gateway.ValueObjects;

public sealed class GatewayRouteTemplate : IEquatable<GatewayRouteTemplate>
{
    public GatewayRouteTemplate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Gateway route template is required.", nameof(value));
        }

        var normalizedValue = value.Trim();

        if (!normalizedValue.StartsWith("/", StringComparison.Ordinal))
        {
            throw new ArgumentException("Gateway route template must start with '/'.", nameof(value));
        }

        Value = normalizedValue;
    }

    public string Value { get; }

    public bool Equals(GatewayRouteTemplate? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => obj is GatewayRouteTemplate other && Equals(other);

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

    public override string ToString() => Value;
}
