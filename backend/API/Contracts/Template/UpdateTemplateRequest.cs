namespace QPhising.Api.Contracts.Template;

public sealed record UpdateTemplateRequest(
    string Name,
    string HtmlContent,
    string? Description,
    IReadOnlyCollection<string>? Tags);
