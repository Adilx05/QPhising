using MediatR;
using QPhising.Application.Contracts.Responses.Template;

namespace QPhising.Application.CQRS.Queries.Template;

public sealed record GetTemplateByIdQuery(Guid TemplateId) : IRequest<TemplateResult>;
