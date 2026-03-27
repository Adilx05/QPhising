using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Templates;

namespace QPhising.Application.Features.Templates.GetTemplateById;

public sealed record GetTemplateByIdQuery(Guid TemplateId) : IRequest<Result<TemplateDetailResponse>>;

public sealed record TemplateDetailResponse(
    Guid Id,
    string Name,
    TemplateType Type,
    string HtmlContent,
    TemplateStatus Status,
    int Version,
    IReadOnlyCollection<string> Variables);
