namespace QPhising.Api.Contracts.Template;

public sealed record UpdateTemplateRequest(
    string Name,
    string Subject,
    string Body,
    string? Description,
    IReadOnlyCollection<string>? Tags);
