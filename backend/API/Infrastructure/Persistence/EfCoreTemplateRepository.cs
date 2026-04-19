using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QPhising.Api.Infrastructure.Persistence.Entities;
using QPhising.Application.Contracts.Abstractions.Template;
using QPhising.Domain.Templates.Aggregates;
using QPhising.Domain.Templates.Enums;
using QPhising.Domain.Templates.ValueObjects;

namespace QPhising.Api.Infrastructure.Persistence;

public sealed class EfCoreTemplateRepository : ITemplateRepository
{
    private static readonly JsonSerializerOptions TagSerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly QPhisingDbContext _dbContext;

    public EfCoreTemplateRepository(QPhisingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TemplateAggregate?> GetByIdAsync(Guid templateId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Templates
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == templateId, cancellationToken);

        return entity is null ? null : ToDomainAggregate(entity);
    }

    public async Task<IReadOnlyCollection<TemplateAggregate>> ListAsync(CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Templates
            .AsNoTracking()
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(ToDomainAggregate).ToArray();
    }

    public async Task SaveAsync(TemplateAggregate aggregate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        var existing = await _dbContext.Templates
            .SingleOrDefaultAsync(x => x.Id == aggregate.Id, cancellationToken);

        if (existing is null)
        {
            _dbContext.Templates.Add(ToEntity(aggregate));
            return;
        }

        existing.Name = aggregate.Name.Value;
        existing.HtmlContent = aggregate.Content.HtmlContent;
        existing.Description = aggregate.Metadata.Description;
        existing.Tags = SerializeTags(aggregate.Metadata.Tags);
        existing.LifecycleState = (int)aggregate.LifecycleState;
        existing.Version = aggregate.Version;
        existing.CreatedAtUtc = aggregate.CreatedAtUtc;
        existing.UpdatedAtUtc = aggregate.UpdatedAtUtc;
        existing.IsDeleted = aggregate.IsDeleted;
        existing.DeletedAtUtc = aggregate.DeletedAtUtc;
        existing.DeletedBy = aggregate.DeletedBy;
    }

    public async Task DeleteAsync(TemplateAggregate aggregate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        var existing = await _dbContext.Templates
            .SingleOrDefaultAsync(x => x.Id == aggregate.Id, cancellationToken);

        if (existing is null)
        {
            return;
        }

        existing.IsDeleted = aggregate.IsDeleted;
        existing.UpdatedAtUtc = aggregate.UpdatedAtUtc;
        existing.DeletedAtUtc = aggregate.DeletedAtUtc;
        existing.DeletedBy = aggregate.DeletedBy;
    }

    private static TemplateAggregate ToDomainAggregate(TemplateEntity entity)
    {
        return TemplateAggregate.Rehydrate(
            entity.Id,
            new TemplateName(entity.Name),
            new TemplateContent(entity.HtmlContent),
            new TemplateMetadata(entity.Description, DeserializeTags(entity.Tags)),
            (TemplateLifecycleState)entity.LifecycleState,
            entity.Version,
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc,
            entity.IsDeleted,
            entity.DeletedAtUtc,
            entity.DeletedBy);
    }

    private static TemplateEntity ToEntity(TemplateAggregate aggregate)
    {
        return new TemplateEntity
        {
            Id = aggregate.Id,
            Name = aggregate.Name.Value,
            HtmlContent = aggregate.Content.HtmlContent,
            Description = aggregate.Metadata.Description,
            Tags = SerializeTags(aggregate.Metadata.Tags),
            LifecycleState = (int)aggregate.LifecycleState,
            Version = aggregate.Version,
            CreatedAtUtc = aggregate.CreatedAtUtc,
            UpdatedAtUtc = aggregate.UpdatedAtUtc,
            IsDeleted = aggregate.IsDeleted,
            DeletedAtUtc = aggregate.DeletedAtUtc,
            DeletedBy = aggregate.DeletedBy
        };
    }

    private static string SerializeTags(IReadOnlyCollection<string> tags)
    {
        return JsonSerializer.Serialize(tags, TagSerializerOptions);
    }

    private static IReadOnlyCollection<string> DeserializeTags(string tagsJson)
    {
        if (string.IsNullOrWhiteSpace(tagsJson))
        {
            return Array.Empty<string>();
        }

        return JsonSerializer.Deserialize<string[]>(tagsJson, TagSerializerOptions) ?? Array.Empty<string>();
    }
}
