using System.Collections.Concurrent;
using QPhising.Application.Contracts.Abstractions.Campaign;
using QPhising.Domain.Campaign.Aggregates;

namespace QPhising.Api.Services.Campaign;

public sealed class InMemoryCampaignRepository : ICampaignRepository
{
    private static readonly ConcurrentDictionary<Guid, CampaignAggregate> Campaigns = new();

    public Task<CampaignAggregate?> GetByIdAsync(Guid campaignId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Campaigns.TryGetValue(campaignId, out var aggregate);
        return Task.FromResult(aggregate);
    }

    public Task<IReadOnlyCollection<CampaignAggregate>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyCollection<CampaignAggregate> campaigns = Campaigns.Values.ToArray();
        return Task.FromResult(campaigns);
    }

    public Task SaveAsync(CampaignAggregate aggregate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        cancellationToken.ThrowIfCancellationRequested();

        Campaigns[aggregate.Id] = aggregate;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(CampaignAggregate aggregate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        cancellationToken.ThrowIfCancellationRequested();

        Campaigns.TryRemove(aggregate.Id, out _);
        return Task.CompletedTask;
    }
}
