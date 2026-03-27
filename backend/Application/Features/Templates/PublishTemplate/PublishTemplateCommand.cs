using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Templates;

namespace QPhising.Application.Features.Templates.PublishTemplate;

public sealed record PublishTemplateCommand(Guid TemplateId) : IRequest<Result<PublishTemplateResponse>>;

public sealed record PublishTemplateResponse(
    Guid Id,
    TemplateStatus Status,
    int Version);
