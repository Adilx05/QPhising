using System;
using System.Collections.Generic;

using QPhising.Domain.Common;

namespace QPhising.Domain.Templates.ValueObjects;

public sealed class TemplateName : ValueObject
{
    public const int MaxLength = 120;

    public TemplateName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Template name is required.", nameof(value));
        }

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
        {
            throw new ArgumentException($"Template name must be at most {MaxLength} characters.", nameof(value));
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
