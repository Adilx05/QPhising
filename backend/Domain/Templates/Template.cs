using System.Text.RegularExpressions;
using QPhising.Domain.Templates.Exceptions;

namespace QPhising.Domain.Templates;

public sealed class Template
{
    private const int MaxNameLength = 200;

    private static readonly Regex PlaceholderRegex = new(
        "\\{\\{\\s*([a-zA-Z][a-zA-Z0-9_]*)\\s*\\}\\}",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly IReadOnlyDictionary<TemplateStatus, TemplateStatus[]> AllowedStatusTransitions =
        new Dictionary<TemplateStatus, TemplateStatus[]>
        {
            [TemplateStatus.Draft] = [TemplateStatus.Published, TemplateStatus.Archived],
            [TemplateStatus.Published] = [TemplateStatus.Archived],
            [TemplateStatus.Archived] = []
        };

    private readonly HashSet<TemplateVariable> _variables;

    private Template(
        Guid id,
        string name,
        TemplateType type,
        string htmlContent,
        TemplateStatus status,
        int version,
        HashSet<TemplateVariable> variables)
    {
        Id = id;
        Name = name;
        Type = type;
        HtmlContent = htmlContent;
        Status = status;
        Version = version;
        _variables = variables;
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    public TemplateType Type { get; private set; }

    public string HtmlContent { get; private set; }

    public TemplateStatus Status { get; private set; }

    public int Version { get; private set; }

    public IReadOnlyCollection<TemplateVariable> Variables => _variables.ToArray();

    public static Template Create(
        string name,
        TemplateType type,
        string htmlContent,
        IEnumerable<string>? explicitVariables = null,
        Guid? id = null)
    {
        string normalizedName = ValidateAndNormalizeName(name);
        string normalizedHtml = ValidateAndNormalizeHtmlContent(htmlContent);
        HashSet<TemplateVariable> variables = ResolveVariables(normalizedHtml, explicitVariables);

        return new Template(
            id ?? Guid.NewGuid(),
            normalizedName,
            type,
            normalizedHtml,
            TemplateStatus.Draft,
            1,
            variables);
    }

    public void UpdateContent(
        string name,
        TemplateType type,
        string htmlContent,
        IEnumerable<string>? explicitVariables = null)
    {
        if (Status == TemplateStatus.Archived)
        {
            throw new TemplateValidationException("Archived templates cannot be modified.");
        }

        Name = ValidateAndNormalizeName(name);
        Type = type;
        HtmlContent = ValidateAndNormalizeHtmlContent(htmlContent);

        _variables.Clear();

        foreach (TemplateVariable variable in ResolveVariables(HtmlContent, explicitVariables))
        {
            _variables.Add(variable);
        }

        Version++;
    }

    public void Publish() => ChangeStatus(TemplateStatus.Published);

    public void Archive() => ChangeStatus(TemplateStatus.Archived);

    public bool UsesVariable(string variableName)
    {
        if (!TemplateVariable.IsValidName(variableName))
        {
            return false;
        }

        string normalized = variableName.Trim();
        return _variables.Contains(TemplateVariable.Create(normalized));
    }

    public static IReadOnlyCollection<TemplateVariable> ExtractVariables(string htmlContent)
    {
        string normalizedHtml = ValidateAndNormalizeHtmlContent(htmlContent);

        MatchCollection matches = PlaceholderRegex.Matches(normalizedHtml);
        HashSet<TemplateVariable> variables = [];

        foreach (Match match in matches)
        {
            if (!match.Success)
            {
                continue;
            }

            string variableName = match.Groups[1].Value;
            variables.Add(TemplateVariable.Create(variableName));
        }

        return variables.ToArray();
    }

    private void ChangeStatus(TemplateStatus requestedStatus)
    {
        if (Status == requestedStatus)
        {
            throw new InvalidTemplateStatusTransitionException(Status, requestedStatus);
        }

        if (!AllowedStatusTransitions.TryGetValue(Status, out TemplateStatus[]? allowedStatuses) ||
            !allowedStatuses.Contains(requestedStatus))
        {
            throw new InvalidTemplateStatusTransitionException(Status, requestedStatus);
        }

        Status = requestedStatus;
        Version++;
    }

    private static string ValidateAndNormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new TemplateValidationException("Template name is required.");
        }

        string normalized = name.Trim();

        if (normalized.Length > MaxNameLength)
        {
            throw new TemplateValidationException($"Template name must be {MaxNameLength} characters or fewer.");
        }

        return normalized;
    }

    private static string ValidateAndNormalizeHtmlContent(string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            throw new TemplateValidationException("Template HTML content is required.");
        }

        return htmlContent.Trim();
    }

    private static HashSet<TemplateVariable> ResolveVariables(string htmlContent, IEnumerable<string>? explicitVariables)
    {
        HashSet<TemplateVariable> contentVariables = [.. ExtractVariables(htmlContent)];

        if (explicitVariables is null)
        {
            return contentVariables;
        }

        HashSet<TemplateVariable> declaredVariables = [];

        foreach (string variableName in explicitVariables)
        {
            declaredVariables.Add(TemplateVariable.Create(variableName));
        }

        foreach (TemplateVariable contentVariable in contentVariables)
        {
            if (!declaredVariables.Contains(contentVariable))
            {
                throw new TemplateValidationException(
                    $"Template content contains undeclared variable '{contentVariable.Name}'.");
            }
        }

        return declaredVariables;
    }
}
