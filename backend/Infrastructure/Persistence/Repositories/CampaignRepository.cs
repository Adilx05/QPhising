using Microsoft.EntityFrameworkCore;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Campaigns;

namespace QPhising.Infrastructure.Persistence.Repositories;

public sealed class CampaignRepository(QPhisingDbContext dbContext) : ICampaignRepository
{
    public async Task<Campaign?> GetByIdAsync(Guid campaignId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Campaigns
            .SingleOrDefaultAsync(campaign => campaign.Id == campaignId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Campaign>> ListAsync(
        CampaignReadCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Campaign> query = dbContext.Campaigns.AsQueryable();

        if (criteria.Statuses is { Count: > 0 })
        {
            query = query.Where(campaign => criteria.Statuses.Contains(campaign.Status));
        }

        if (criteria.TemplateTypes is { Count: > 0 })
        {
            query = query.Where(campaign => criteria.TemplateTypes.Contains(campaign.TemplateType));
        }

        if (criteria.StartsOnOrAfter.HasValue)
        {
            query = query.Where(campaign => campaign.StartDate >= criteria.StartsOnOrAfter.Value);
        }

        if (criteria.EndsOnOrBefore.HasValue)
        {
            query = query.Where(campaign => campaign.EndDate <= criteria.EndsOnOrBefore.Value);
        }

        query = query.OrderBy(campaign => campaign.StartDate).ThenBy(campaign => campaign.Id);

        if (criteria.Skip is > 0)
        {
            query = query.Skip(criteria.Skip.Value);
        }

        if (criteria.Take is > 0)
        {
            query = query.Take(criteria.Take.Value);
        }

        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Campaign>> ListOverlappingWindowAsync(
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Campaigns
            .Where(campaign => campaign.StartDate <= windowEnd && campaign.EndDate >= windowStart)
            .OrderBy(campaign => campaign.StartDate)
            .ThenBy(campaign => campaign.Id)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddAsync(Campaign campaign, CancellationToken cancellationToken = default)
    {
        await dbContext.Campaigns.AddAsync(campaign, cancellationToken);
    }

    public void Update(Campaign campaign)
    {
        dbContext.Campaigns.Update(campaign);
    }
}
