using Microsoft.Extensions.Options;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions;
using QPhising.Application.Features.Tracking.GenerateTrackingLink;
using QPhising.Application.Features.Tracking.ProcessTrackingClick;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Campaigns;
using QPhising.Infrastructure.Security;
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
        var handler = new GenerateTrackingLinkCommandHandler(guard, CreateTokenService());

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
        var handler = new ProcessTrackingClickCommandHandler(guard, CreateTokenService(), new InMemoryTrackingClickRepository(), new NoOpUnitOfWork());

        var result = await handler.Handle(
            new ProcessTrackingClickCommand(campaign.Id, "tracking-token", "127.0.0.1", "integration-test-agent"),
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
        var tokenService = CreateTokenService();
        var handler = new GenerateTrackingLinkCommandHandler(guard, tokenService);

        var result = await handler.Handle(
            new GenerateTrackingLinkCommand(campaign.Id, "employee@company.test"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(campaign.Id, result.Value!.CampaignId);
        Assert.StartsWith("/api/v1/tracking/click/", result.Value.TrackingPath);
        Assert.Equal("HS256", result.Value.SignatureAlgorithm);

        var validation = tokenService.ValidateToken(result.Value.TrackingToken, campaign.Id);
        Assert.True(validation.IsValid);
        Assert.NotNull(validation.Claims);
        Assert.Equal("employee@company.test", validation.Claims!.RecipientEmail);
    }

    [Fact]
    public async Task ProcessTrackingClick_Should_Reject_When_TrackingToken_Is_Tampered()
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = Campaign.Create(
            "Security Simulation",
            TemplateType.Email,
            "<h1>Simulation</h1>",
            now.AddDays(-1),
            now.AddDays(2));

        var repository = new InMemoryCampaignRepository(campaign);
        var guard = new CampaignInteractionGuard(repository);
        var tokenService = CreateTokenService();
        var issueResult = tokenService.IssueToken(new TrackingTokenIssueRequest(campaign.Id, "employee@company.test", Guid.NewGuid().ToString("N")));

        string tamperedToken = issueResult.Token[..^1] + (issueResult.Token[^1] == 'a' ? 'b' : 'a');
        var handler = new ProcessTrackingClickCommandHandler(guard, tokenService, new InMemoryTrackingClickRepository(), new NoOpUnitOfWork());

        var result = await handler.Handle(
            new ProcessTrackingClickCommand(campaign.Id, tamperedToken, "127.0.0.1", "integration-test-agent"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("signature is invalid", result.Errors.Single(), StringComparison.OrdinalIgnoreCase);
    }

    private static ITrackingTokenService CreateTokenService()
    {
        var options = Options.Create(new TrackingTokenOptions
        {
            SigningKey = "integration-test-signing-key-minimum-32chars",
            ExpirationMinutes = 30,
            Version = 1
        });

        return new HmacTrackingTokenService(options);
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

    private sealed class InMemoryTrackingClickRepository : ITrackingClickRepository
    {
        public Task AddAsync(QPhising.Domain.Tracking.TrackingClick click, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }
    }
}
