using Microsoft.EntityFrameworkCore;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Templates;

namespace QPhising.Infrastructure.Persistence.Repositories;

public sealed class TemplateRepository(QPhisingDbContext dbContext) : ITemplateRepository
{
    public async Task<Template?> GetByIdAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Templates
            .Include(template => template.Variables)
            .SingleOrDefaultAsync(template => template.Id == templateId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Template>> ListAsync(
        TemplateReadCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Template> query = dbContext.Templates
            .Include(template => template.Variables)
            .AsQueryable();

        if (criteria.Status.HasValue)
        {
            query = query.Where(template => template.Status == criteria.Status.Value);
        }

        if (criteria.Type.HasValue)
        {
            query = query.Where(template => template.Type == criteria.Type.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            string searchTerm = criteria.SearchTerm.Trim();
            query = query.Where(template => EF.Functions.ILike(template.Name, $"%{searchTerm}%"));
        }

        int skip = (criteria.PageNumber - 1) * criteria.PageSize;

        query = query
            .OrderBy(template => template.Name)
            .ThenBy(template => template.Id)
            .Skip(skip)
            .Take(criteria.PageSize);

        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<Template?> GetPublishedByNameAsync(string templateName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(templateName))
        {
            return null;
        }

        string normalizedTemplateName = templateName.Trim();

        return await dbContext.Templates
            .Include(template => template.Variables)
            .SingleOrDefaultAsync(
                template => template.Name == normalizedTemplateName && template.Status == TemplateStatus.Published,
                cancellationToken);
    }

    public async Task AddAsync(Template template, CancellationToken cancellationToken = default)
    {
        await dbContext.Templates.AddAsync(template, cancellationToken);
    }

    public void Update(Template template)
    {
        dbContext.Templates.Update(template);
    }
}
