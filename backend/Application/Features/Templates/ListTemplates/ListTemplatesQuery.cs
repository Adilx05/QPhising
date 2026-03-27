using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Templates;

namespace QPhising.Application.Features.Templates.ListTemplates;

public sealed record ListTemplatesQuery(
    TemplateStatus? Status = null,
    TemplateType? Type = null,
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<ListTemplatesResponse>>;

public sealed record ListTemplatesResponse(
    int PageNumber,
    int PageSize,
    IReadOnlyCollection<TemplateListItemResponse> Items);

public sealed record TemplateListItemResponse(
    Guid Id,
    string Name,
    TemplateType Type,
    TemplateStatus Status,
    int Version,
    IReadOnlyCollection<string> Variables);
