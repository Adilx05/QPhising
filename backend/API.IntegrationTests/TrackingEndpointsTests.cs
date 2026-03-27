using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using QPhising.Application.Common.Abstractions;
using QPhising.API.Controllers;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Campaigns;
using QPhising.Domain.Tracking;
using QPhising.Infrastructure.Security;
using Xunit;

namespace QPhising.API.IntegrationTests;

public sealed class TrackingEndpointsTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public TrackingEndpointsTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GenerateTrackingLink_Should_Reject_Unauthenticated_Request()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/tracking/links", new
        {
            campaignId = Guid.NewGuid(),
            recipientEmail = "employee@company.test"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GenerateTrackingLink_Should_Reject_Viewer_Role()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Test");
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.RolesHeader, AuthorizationPolicies.Viewer);

        var response = await client.PostAsJsonAsync("/api/tracking/links", new
        {
            campaignId = Guid.NewGuid(),
            recipientEmail = "employee@company.test"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GenerateTrackingLink_Should_Return_Created_For_Operator()
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = Campaign.Create(
            "Tracking Campaign",
            TemplateType.Email,
            "<p>Training</p>",
            now.AddDays(-1),
            now.AddDays(7));

        using var client = CreateClientWithCampaign(campaign);
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Test");
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.RolesHeader, AuthorizationPolicies.Operator);

        var response = await client.PostAsJsonAsync("/api/tracking/links", new
        {
            campaignId = campaign.Id,
            recipientEmail = "employee@company.test"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<GenerateTrackingLinkApiResponse>();
        Assert.NotNull(payload);
        Assert.Equal(campaign.Id, payload!.CampaignId);
        Assert.StartsWith("http://gateway:8080/api/v1/tracking/click/", payload.TrackingUrl);
        Assert.False(string.IsNullOrWhiteSpace(payload.TrackingToken));
    }

    [Fact]
    public async Task ProcessTrackingClick_Should_Accept_Anonymous_Request_And_Persist_Metadata()
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = Campaign.Create(
            "Tracking Campaign",
            TemplateType.Email,
            "<p>Training</p>",
            now.AddDays(-1),
            now.AddDays(7));

        var clickRepository = new InMemoryTrackingClickRepository();
        var tokenService = CreateTokenService();
        var token = tokenService.IssueToken(
            new TrackingTokenIssueRequest(campaign.Id, "employee@company.test", Guid.NewGuid().ToString("N"))).Token;

        using var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<ICampaignRepository>(new InMemoryCampaignRepository(campaign));
                services.AddSingleton<ITrackingClickRepository>(clickRepository);
            });
        }).CreateClient();

        client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "tracking-endpoint-tests");

        var response = await client.GetAsync($"/api/tracking/click/{campaign.Id}/{token}?fingerprint=device-1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ProcessTrackingClickApiResponse>();
        Assert.NotNull(payload);
        Assert.Equal(campaign.Id, payload!.CampaignId);
        Assert.True(payload.Accepted);
        Assert.Equal("device-1", payload.Fingerprint);
        Assert.NotEqual(Guid.Empty, payload.ClickId);
        Assert.Single(clickRepository.Items);
        Assert.Equal(payload.ClickId, clickRepository.Items.Single().Id);
    }

    private static ITrackingTokenService CreateTokenService()
    {
        return new HmacTrackingTokenService(Microsoft.Extensions.Options.Options.Create(new TrackingTokenOptions
        {
            SigningKey = "integration-test-signing-key-minimum-32chars",
            ExpirationMinutes = 30,
            Version = 1
        }));
    }

    private HttpClient CreateClientWithCampaign(Campaign campaign)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<ICampaignRepository>(new InMemoryCampaignRepository(campaign));
            });
        }).CreateClient();
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
            return Task.FromResult((IReadOnlyCollection<Campaign>)_campaigns.Values.ToArray());
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
        private readonly List<TrackingClick> _items = [];

        public IReadOnlyCollection<TrackingClick> Items => _items.AsReadOnly();

        public Task AddAsync(TrackingClick click, CancellationToken cancellationToken = default)
        {
            _items.Add(click);
            return Task.CompletedTask;
        }
    }
}
