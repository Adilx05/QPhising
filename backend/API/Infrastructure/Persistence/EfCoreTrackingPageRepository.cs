using Microsoft.EntityFrameworkCore;
using QPhising.Api.Infrastructure.Persistence.Mapping;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Domain.Tracking.Aggregates;

namespace QPhising.Api.Infrastructure.Persistence;

public sealed class EfCoreTrackingPageRepository : ITrackingPageRepository
{
    private readonly QPhisingDbContext _dbContext;

    public EfCoreTrackingPageRepository(QPhisingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TrackingPageAggregate?> GetByIdAsync(Guid trackingPageId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.TrackingPages
            .AsNoTracking()
            .SingleOrDefaultAsync(page => page.Id == trackingPageId, cancellationToken);

        return entity?.ToAggregate();
    }

    public async Task<TrackingPageAggregate?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var normalizedSlug = slug.Trim();

        var entity = await _dbContext.TrackingPages
            .AsNoTracking()
            .SingleOrDefaultAsync(page => page.Slug == normalizedSlug, cancellationToken);

        return entity?.ToAggregate();
    }

    public async Task<IReadOnlyCollection<TrackingPageAggregate>> ListAsync(CancellationToken cancellationToken)
    {
        var entities = await _dbContext.TrackingPages
            .AsNoTracking()
            .OrderByDescending(page => page.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(entity => entity.ToAggregate()).ToArray();
    }

    public Task<bool> SlugExistsAsync(string slug, Guid? excludingTrackingPageId, CancellationToken cancellationToken)
    {
        var normalizedSlug = slug.Trim();

        return _dbContext.TrackingPages.AnyAsync(
            page => page.Slug == normalizedSlug && (!excludingTrackingPageId.HasValue || page.Id != excludingTrackingPageId.Value),
            cancellationToken);
    }

    public async Task SaveAsync(TrackingPageAggregate aggregate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        var existing = await _dbContext.TrackingPages
            .SingleOrDefaultAsync(page => page.Id == aggregate.Id, cancellationToken);

        if (existing is null)
        {
            _dbContext.TrackingPages.Add(aggregate.ToEntity());
            return;
        }

        existing.Slug = aggregate.Slug.Value;
        existing.Title = aggregate.Title;
        existing.Description = aggregate.Description;
        existing.OwnerId = aggregate.OwnerId;
        existing.TemplateId = aggregate.TemplateId;
        existing.CustomHtmlContent = aggregate.CustomHtmlContent;
        existing.ValidFromUtc = aggregate.ValidFromUtc;
        existing.ValidUntilUtc = aggregate.ValidUntilUtc;
        existing.PublishState = (int)aggregate.PublishState;
        existing.RetentionDays = aggregate.Settings?.RetentionDays;
        existing.MaskIpAddress = aggregate.Settings?.MaskIpAddress;
        existing.EnableBotFiltering = aggregate.Settings?.EnableBotFiltering;
        existing.CaptureUtmParameters = aggregate.Settings?.CaptureUtmParameters;
        existing.CreatedAtUtc = aggregate.CreatedAtUtc;
        existing.UpdatedAtUtc = aggregate.UpdatedAtUtc;
    }

    public async Task DeleteAsync(TrackingPageAggregate aggregate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        var existing = await _dbContext.TrackingPages
            .SingleOrDefaultAsync(page => page.Id == aggregate.Id, cancellationToken);

        if (existing is null)
        {
            return;
        }

        _dbContext.TrackingPages.Remove(existing);
    }
}
