using Microsoft.EntityFrameworkCore;
using QPhising.Api.Infrastructure.Persistence.Entities;
using QPhising.Application.Contracts.Abstractions.Campaign;
using QPhising.Domain.Campaign.Aggregates;
using QPhising.Domain.Campaign.Entities;
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
            .Include(x => x.Targets)
            .SingleOrDefaultAsync(x => x.Id == campaignId, cancellationToken);

        return entity is null ? null : ToDomainAggregate(entity);
    }

    public async Task<IReadOnlyCollection<CampaignAggregate>> ListAsync(CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Campaigns
            .AsNoTracking()
            .Include(x => x.Targets)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(ToDomainAggregate).ToArray();
    }

    public async Task SaveAsync(CampaignAggregate aggregate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        var existing = await _dbContext.Campaigns
            .Include(x => x.Targets)
            .SingleOrDefaultAsync(x => x.Id == aggregate.Id, cancellationToken);

        if (existing is null)
        {
            _dbContext.Campaigns.Add(ToEntity(aggregate));
            return;
        }

        existing.Name = aggregate.Name.Value;
        existing.TemplateId = aggregate.TemplateId;
        existing.LifecycleState = (int)aggregate.LifecycleState;
        existing.ScheduleStartsAtUtc = aggregate.ScheduleWindow?.StartsAtUtc;
        existing.ScheduleEndsAtUtc = aggregate.ScheduleWindow?.EndsAtUtc;
        existing.CreatedAtUtc = aggregate.CreatedAtUtc;
        existing.UpdatedAtUtc = aggregate.UpdatedAtUtc;

        existing.Targets.Clear();
        foreach (var target in aggregate.Targets)
        {
            existing.Targets.Add(new CampaignTargetEntity
            {
                Id = target.Id,
                CampaignId = aggregate.Id,
                EmailAddress = target.EmailAddress
            });
        }
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

        _dbContext.Campaigns.Remove(existing);
    }

    private static CampaignAggregate ToDomainAggregate(CampaignEntity entity)
    {
        var scheduleWindow = entity.ScheduleStartsAtUtc.HasValue
            ? new CampaignScheduleWindow(entity.ScheduleStartsAtUtc.Value, entity.ScheduleEndsAtUtc)
            : null;

        var targets = entity.Targets
            .Select(target => new CampaignTarget(target.Id, target.EmailAddress))
            .ToArray();

        return CampaignAggregate.Rehydrate(
            entity.Id,
            new CampaignName(entity.Name),
            entity.TemplateId,
            (CampaignLifecycleState)entity.LifecycleState,
            scheduleWindow,
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc,
            targets);
    }

    private static CampaignEntity ToEntity(CampaignAggregate aggregate)
    {
        return new CampaignEntity
        {
            Id = aggregate.Id,
            Name = aggregate.Name.Value,
            TemplateId = aggregate.TemplateId,
            LifecycleState = (int)aggregate.LifecycleState,
            ScheduleStartsAtUtc = aggregate.ScheduleWindow?.StartsAtUtc,
            ScheduleEndsAtUtc = aggregate.ScheduleWindow?.EndsAtUtc,
            CreatedAtUtc = aggregate.CreatedAtUtc,
            UpdatedAtUtc = aggregate.UpdatedAtUtc,
            Targets = aggregate.Targets.Select(target => new CampaignTargetEntity
            {
                Id = target.Id,
                CampaignId = aggregate.Id,
                EmailAddress = target.EmailAddress
            }).ToList()
        };
    }
}
