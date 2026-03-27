using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Templates;

namespace QPhising.Application.Features.Templates.UpdateTemplate;

public sealed record UpdateTemplateCommand(
    Guid TemplateId,
    string Name,
    TemplateType Type,
    string HtmlContent,
    IReadOnlyCollection<string>? Variables = null) : IRequest<Result<UpdateTemplateResponse>>;

public sealed record UpdateTemplateResponse(
    Guid Id,
    string Name,
    TemplateType Type,
    string HtmlContent,
    TemplateStatus Status,
    int Version,
    IReadOnlyCollection<string> Variables);
