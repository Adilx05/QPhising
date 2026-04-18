using QPhising.Domain.Templates.Aggregates;

namespace QPhising.Application.Contracts.Abstractions.Template;

public interface ITemplateRepository
{
    Task<TemplateAggregate?> GetByIdAsync(Guid templateId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TemplateAggregate>> ListAsync(CancellationToken cancellationToken);

    Task SaveAsync(TemplateAggregate aggregate, CancellationToken cancellationToken);

    Task DeleteAsync(TemplateAggregate aggregate, CancellationToken cancellationToken);
}
