using Microsoft.EntityFrameworkCore;
using QPhising.Api.Infrastructure.Persistence.Entities;
using QPhising.Application.Contracts.Abstractions.Campaign;
using QPhising.Domain.Campaign.Aggregates;
using QPhising.Domain.Campaign.Enums;
using QPhising.Domain.Campaign.ValueObjects;

namespace QPhising.Api.Infrastructure.Persistence;

public sealed class EfCoreCampaignRepository : ICampaignRepository
{
    private readonly QPhisingDbContext _dbContext;

    public EfCoreCampaignRepository(QPhisingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CampaignAggregate?> GetByIdAsync(Guid campaignId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Campaigns
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == campaignId, cancellationToken);

        return entity is null ? null : ToDomainAggregate(entity);
    }
    
    public async Task<CampaignAggregate?> GetByTrackingPageIdAsync(Guid trackingPageId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Campaigns
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.TrackingPageId == trackingPageId, cancellationToken);

        return entity is null ? null : ToDomainAggregate(entity);
    }

    public async Task<IReadOnlyCollection<CampaignAggregate>> ListAsync(CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Campaigns
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(ToDomainAggregate).ToArray();
    }

    public async Task SaveAsync(CampaignAggregate aggregate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        var existing = await _dbContext.Campaigns
            .SingleOrDefaultAsync(x => x.Id == aggregate.Id, cancellationToken);

        if (existing is null)
        {
            _dbContext.Campaigns.Add(ToEntity(aggregate));
            return;
        }

        existing.Name = aggregate.Name.Value;
        existing.TrackingPageId = aggregate.TrackingPageId;
        existing.TemplateId = aggregate.TemplateId;
        existing.LifecycleState = (int)aggregate.LifecycleState;
        existing.ScheduleStartsAtUtc = aggregate.ScheduleWindow?.StartsAtUtc;
        existing.ScheduleEndsAtUtc = aggregate.ScheduleWindow?.EndsAtUtc;
        existing.CreatedAtUtc = aggregate.CreatedAtUtc;
        existing.UpdatedAtUtc = aggregate.UpdatedAtUtc;
        existing.IsDeleted = aggregate.IsDeleted;
        existing.DeletedAtUtc = aggregate.DeletedAtUtc;
        existing.DeletedBy = aggregate.DeletedBy;
    }

    public async Task DeleteAsync(CampaignAggregate aggregate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        var existing = await _dbContext.Campaigns
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

    private static CampaignAggregate ToDomainAggregate(CampaignEntity entity)
    {
        var scheduleWindow = entity.ScheduleStartsAtUtc.HasValue
            ? new CampaignScheduleWindow(entity.ScheduleStartsAtUtc.Value, entity.ScheduleEndsAtUtc)
            : null;

        return CampaignAggregate.Rehydrate(
            entity.Id,
            new CampaignName(entity.Name),
            entity.TrackingPageId,
            entity.TemplateId,
            (CampaignLifecycleState)entity.LifecycleState,
            scheduleWindow,
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc,
            entity.IsDeleted,
            entity.DeletedAtUtc,
            entity.DeletedBy);
    }

    private static CampaignEntity ToEntity(CampaignAggregate aggregate)
    {
        return new CampaignEntity
        {
            Id = aggregate.Id,
            Name = aggregate.Name.Value,
            TrackingPageId = aggregate.TrackingPageId,
            TemplateId = aggregate.TemplateId,
            LifecycleState = (int)aggregate.LifecycleState,
            ScheduleStartsAtUtc = aggregate.ScheduleWindow?.StartsAtUtc,
            ScheduleEndsAtUtc = aggregate.ScheduleWindow?.EndsAtUtc,
            CreatedAtUtc = aggregate.CreatedAtUtc,
            UpdatedAtUtc = aggregate.UpdatedAtUtc,
            IsDeleted = aggregate.IsDeleted,
            DeletedAtUtc = aggregate.DeletedAtUtc,
            DeletedBy = aggregate.DeletedBy
        };
    }
}
