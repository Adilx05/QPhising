using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Templates;

namespace QPhising.Application.Features.Templates.ArchiveTemplate;

public sealed record ArchiveTemplateCommand(Guid TemplateId) : IRequest<Result<ArchiveTemplateResponse>>;

public sealed record ArchiveTemplateResponse(
    Guid Id,
    TemplateStatus Status,
    int Version);
