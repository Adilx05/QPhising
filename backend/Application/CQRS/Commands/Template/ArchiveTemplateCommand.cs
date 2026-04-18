using QPhising.Application.Contracts.Abstractions.Persistence;
using QPhising.Application.Contracts.Responses.Template;

namespace QPhising.Application.CQRS.Commands.Template;

public sealed record ArchiveTemplateCommand(Guid TemplateId) : ITransactionalRequest<TemplateResult>;
