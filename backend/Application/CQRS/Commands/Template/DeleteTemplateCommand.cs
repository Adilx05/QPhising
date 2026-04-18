using MediatR;
using QPhising.Application.Contracts.Abstractions.Persistence;

namespace QPhising.Application.CQRS.Commands.Template;

public sealed record DeleteTemplateCommand(Guid TemplateId) : ITransactionalRequest<Unit>;
