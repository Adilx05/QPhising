using System.Text;
using System.Text.RegularExpressions;

namespace QPhising.Application.ApiConventions;

/// <summary>
/// Generates and validates canonical OpenAPI operation identifiers.
/// </summary>
public static partial class OpenApiOperationIdConvention
{
    private static readonly Regex ValidOperationIdPattern = OperationIdRegex();

    public static string Create(string resourceName, string actionName)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            throw new ArgumentException("Resource name is required.", nameof(resourceName));
        }

        if (string.IsNullOrWhiteSpace(actionName))
        {
            throw new ArgumentException("Action name is required.", nameof(actionName));
        }

        var normalizedResourceName = ToPascalCase(resourceName);
        var normalizedActionName = ToPascalCase(actionName);
        var operationId = $"{normalizedActionName}{normalizedResourceName}";

        if (!ValidOperationIdPattern.IsMatch(operationId))
        {
            throw new ArgumentException(
                "Operation ID must be PascalCase alphanumeric and start with a letter.",
                nameof(actionName));
        }

        return operationId;
    }

    public static bool IsValid(string operationId)
    {
        if (string.IsNullOrWhiteSpace(operationId))
        {
            return false;
        }

        return ValidOperationIdPattern.IsMatch(operationId.Trim());
    }

    private static string ToPascalCase(string value)
    {
        var segments = value
            .Split(['-', '_', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(segment => segment.ToLowerInvariant());

        var builder = new StringBuilder();
        foreach (var segment in segments)
        {
            if (segment.Length == 0)
            {
                continue;
            }

            builder.Append(char.ToUpperInvariant(segment[0]));
            if (segment.Length > 1)
            {
                builder.Append(segment[1..]);
            }
        }

        return builder.ToString();
    }

    [GeneratedRegex("^[A-Z][A-Za-z0-9]*$")]
    private static partial Regex OperationIdRegex();
}
