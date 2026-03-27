using QPhising.Domain.Templates;

namespace QPhising.Domain.Abstractions;

public sealed record TemplateReadCriteria(
    TemplateStatus? Status = null,
    TemplateType? Type = null,
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 20);
