using QPhising.Domain.Templates;

namespace QPhising.Domain.Abstractions;

public interface ITemplateRepository
{
    Task<Template?> GetByIdAsync(Guid templateId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Template>> ListAsync(
        TemplateReadCriteria criteria,
        CancellationToken cancellationToken = default);

    Task<Template?> GetPublishedByNameAsync(string templateName, CancellationToken cancellationToken = default);

    Task AddAsync(Template template, CancellationToken cancellationToken = default);

    void Update(Template template);
}
