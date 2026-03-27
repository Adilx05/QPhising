using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using QPhising.API.Controllers;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Campaigns;
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
}
