using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Templates;

namespace QPhising.Application.Features.Templates.CreateTemplate;

public sealed record CreateTemplateCommand(
    string Name,
    TemplateType Type,
    string HtmlContent,
    IReadOnlyCollection<string>? Variables = null) : IRequest<Result<CreateTemplateResponse>>;

public sealed record CreateTemplateResponse(
    Guid Id,
    string Name,
    TemplateType Type,
    string HtmlContent,
    TemplateStatus Status,
    int Version,
    IReadOnlyCollection<string> Variables);
