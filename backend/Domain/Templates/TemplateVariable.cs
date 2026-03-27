using System.Text.RegularExpressions;
using QPhising.Domain.Templates.Exceptions;

namespace QPhising.Domain.Templates;

public sealed class TemplateVariable : IEquatable<TemplateVariable>
{
    private static readonly Regex VariableNameRegex = new(
        "^[a-zA-Z][a-zA-Z0-9_]{0,63}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private TemplateVariable(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public string Placeholder => $"{{{{{Name}}}}}";

    public static TemplateVariable Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new TemplateValidationException("Template variable name is required.");
        }

        string normalized = name.Trim();

        if (!VariableNameRegex.IsMatch(normalized))
        {
            throw new TemplateValidationException(
                "Template variable name must start with a letter and contain only letters, numbers, or underscore (max 64 chars).");
        }

        return new TemplateVariable(normalized);
    }

    public static bool IsValidName(string name) =>
        !string.IsNullOrWhiteSpace(name) && VariableNameRegex.IsMatch(name.Trim());

    public bool Equals(TemplateVariable? other) =>
        other is not null && string.Equals(Name, other.Name, StringComparison.Ordinal);

    public override bool Equals(object? obj) => Equals(obj as TemplateVariable);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Name);
}
