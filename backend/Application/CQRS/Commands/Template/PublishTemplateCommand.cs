using QPhising.Application.Contracts.Abstractions.Persistence;
using QPhising.Application.Contracts.Responses.Template;

namespace QPhising.Application.CQRS.Commands.Template;

public sealed record PublishTemplateCommand(Guid TemplateId) : ITransactionalRequest<TemplateResult>;
