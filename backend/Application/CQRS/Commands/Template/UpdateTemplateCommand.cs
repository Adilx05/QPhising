using QPhising.Application.Contracts.Abstractions.Persistence;
using QPhising.Application.Contracts.Responses.Template;

namespace QPhising.Application.CQRS.Commands.Template;

public sealed record UpdateTemplateCommand(
    Guid TemplateId,
    string Name,
    string HtmlContent,
    string? Description,
    IReadOnlyCollection<string>? Tags) : ITransactionalRequest<TemplateResult>;
