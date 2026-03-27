using QPhising.Application.Common;
using QPhising.Application.Features.Tracking.GenerateTrackingLink;
using QPhising.Application.Features.Tracking.ProcessTrackingClick;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Campaigns;
using Xunit;

namespace QPhising.API.IntegrationTests;

public sealed class TrackingInteractionGuardTests
{
    [Fact]
    public async Task GenerateTrackingLink_Should_Reject_When_Campaign_Is_Expired()
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = Campaign.Create(
            "Expired Campaign",
            TemplateType.Email,
            "<h1>Phishing Simulation</h1>",
            now.AddDays(-10),
            now.AddDays(-1));

        var repository = new InMemoryCampaignRepository(campaign);
        var guard = new CampaignInteractionGuard(repository);
        var handler = new GenerateTrackingLinkCommandHandler(guard);

        var result = await handler.Handle(
            new GenerateTrackingLinkCommand(campaign.Id, "employee@company.test"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Tracking interactions are blocked", result.Errors.Single());
    }

    [Fact]
    public async Task ProcessTrackingClick_Should_Reject_When_Campaign_Is_Expired()
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = Campaign.Create(
            "Expired Campaign",
            TemplateType.Email,
            "<h1>Phishing Simulation</h1>",
            now.AddDays(-5),
            now.AddDays(-1));

        var repository = new InMemoryCampaignRepository(campaign);
        var guard = new CampaignInteractionGuard(repository);
        var handler = new ProcessTrackingClickCommandHandler(guard);

        var result = await handler.Handle(
            new ProcessTrackingClickCommand(campaign.Id, "tracking-token"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Tracking interactions are blocked", result.Errors.Single());
    }

    [Fact]
    public async Task GenerateTrackingLink_Should_Succeed_When_Campaign_Is_Not_Expired()
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = Campaign.Create(
            "Q3 Simulation",
            TemplateType.Email,
            "<h1>Phishing Simulation</h1>",
            now.AddDays(-1),
            now.AddDays(5));

        var repository = new InMemoryCampaignRepository(campaign);
        var guard = new CampaignInteractionGuard(repository);
        var handler = new GenerateTrackingLinkCommandHandler(guard);

        var result = await handler.Handle(
            new GenerateTrackingLinkCommand(campaign.Id, "employee@company.test"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(campaign.Id, result.Value!.CampaignId);
        Assert.StartsWith("/api/v1/tracking/click/", result.Value.TrackingPath);
    }

    private sealed class InMemoryCampaignRepository : ICampaignRepository
    {
        private readonly Dictionary<Guid, Campaign> _campaigns;

        public InMemoryCampaignRepository(params Campaign[] campaigns)
        {
            _campaigns = campaigns.ToDictionary(c => c.Id, c => c);
        }

        public Task<Campaign?> GetByIdAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            _campaigns.TryGetValue(campaignId, out Campaign? campaign);
            return Task.FromResult(campaign);
        }

        public Task<IReadOnlyCollection<Campaign>> ListAsync(CampaignReadCriteria criteria, CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<Campaign> result = _campaigns.Values.ToArray();
            return Task.FromResult(result);
        }

        public Task<IReadOnlyCollection<Campaign>> ListOverlappingWindowAsync(
            DateTimeOffset windowStart,
            DateTimeOffset windowEnd,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<Campaign> result = _campaigns.Values
                .Where(campaign => campaign.StartDate <= windowEnd && campaign.EndDate >= windowStart)
                .ToArray();

            return Task.FromResult(result);
        }

        public Task AddAsync(Campaign campaign, CancellationToken cancellationToken = default)
        {
            _campaigns[campaign.Id] = campaign;
            return Task.CompletedTask;
        }

        public void Update(Campaign campaign)
        {
            _campaigns[campaign.Id] = campaign;
        }
    }
}
