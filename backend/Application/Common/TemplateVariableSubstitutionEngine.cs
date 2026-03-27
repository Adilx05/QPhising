using System.Net;
using System.Text.RegularExpressions;
using QPhising.Application.Common.Abstractions;
using QPhising.Domain.Templates;

namespace QPhising.Application.Common;

public sealed partial class TemplateVariableSubstitutionEngine : ITemplateVariableSubstitutionEngine
{
    public Result<string> Render(
        string htmlContent,
        IReadOnlyCollection<string> allowedVariables,
        IReadOnlyDictionary<string, string> variableValues)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            return Result<string>.Failure("Template HTML content is required.");
        }

        if (allowedVariables.Count == 0)
        {
            return Result<string>.Failure("At least one template variable must be declared for rendering.");
        }

        HashSet<string> allowedSet = [];
        foreach (string variable in allowedVariables)
        {
            if (!TemplateVariable.IsValidName(variable))
            {
                return Result<string>.Failure($"Template variable '{variable}' is invalid.");
            }

            allowedSet.Add(variable.Trim());
        }

        List<string> unapprovedInputs = variableValues.Keys
            .Where(key => !allowedSet.Contains(key.Trim()))
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToList();

        if (unapprovedInputs.Count > 0)
        {
            return Result<string>.Failure(
                $"Provided values include undeclared variables: {string.Join(", ", unapprovedInputs)}.");
        }

        MatchCollection placeholders = PlaceholderPattern().Matches(htmlContent);
        HashSet<string> placeholdersInTemplate = [];

        foreach (Match placeholder in placeholders)
        {
            if (!placeholder.Success)
            {
                continue;
            }

            string variableName = placeholder.Groups["variable"].Value.Trim();

            if (!allowedSet.Contains(variableName))
            {
                return Result<string>.Failure(
                    $"Template contains undeclared placeholder '{{{{{variableName}}}}}'.");
            }

            placeholdersInTemplate.Add(variableName);
        }

        List<string> missingValues = placeholdersInTemplate
            .Where(placeholderName => !variableValues.Keys.Any(key =>
                string.Equals(key.Trim(), placeholderName, StringComparison.Ordinal)))
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToList();

        if (missingValues.Count > 0)
        {
            return Result<string>.Failure(
                $"Missing values for placeholders: {string.Join(", ", missingValues)}.");
        }

        string renderedHtml = PlaceholderPattern().Replace(htmlContent, match =>
        {
            string variableName = match.Groups["variable"].Value.Trim();
            string value = variableValues.First(kvp =>
                string.Equals(kvp.Key.Trim(), variableName, StringComparison.Ordinal)).Value;
            return WebUtility.HtmlEncode(value);
        });

        return Result<string>.Success(renderedHtml.Trim());
    }

    [GeneratedRegex("\\{\\{\\s*(?<variable>[a-zA-Z][a-zA-Z0-9_]*)\\s*\\}\\}",
        RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex PlaceholderPattern();
}
