namespace QPhising.Api.Contracts.Template;

public sealed record CreateTemplateRequest(
    string Name,
    string Subject,
    string Body,
    string? Description,
    IReadOnlyCollection<string>? Tags);
