namespace QPhising.Api.Contracts.Template;

public sealed record CreateTemplateRequest(
    string Name,
    string HtmlContent,
    string? Description,
    IReadOnlyCollection<string>? Tags);
