using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using QPhising.Api.Tests.Infrastructure;
using Xunit;

namespace QPhising.Api.Tests;

public sealed class TrackingEndpointsIntegrationTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;
    private static string UniqueSuffix => Guid.NewGuid().ToString("N");

    public TrackingEndpointsIntegrationTests(TestApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TrackingCrud_VisitIngestion_AnalyticsOverview_ShouldWorkWithRepresentativeDataset()
    {
        var client = _factory.CreateClient();
        var slug = $"quarterly-report-{UniqueSuffix}";

        var createResponse = await client.PostAsJsonAsync("/api/tracking/pages", new
        {
            slug,
            title = "Quarterly Report",
            description = "Investor report tracking page",
            destinationUrl = "https://example.com/reports/q1",
            ownerId = "ops-admin",
            retentionDays = 30,
            captureIpAddress = true,
            enableBotFiltering = true,
            captureUtmParameters = true
        });

        createResponse.EnsureSuccessStatusCode();
        var created = JsonNode.Parse(await createResponse.Content.ReadAsStringAsync())!.AsObject();
        var trackingPageId = created["id"]!.GetValue<Guid>();

        var publishResponse = await client.PostAsync($"/api/tracking/pages/{trackingPageId}/publish", content: null);
        publishResponse.EnsureSuccessStatusCode();

        var publicLandingResponse = await client.GetAsync($"/p/{slug}");
        publicLandingResponse.EnsureSuccessStatusCode();

        var now = DateTimeOffset.UtcNow;
        var visitOneResponse = await client.PostAsJsonAsync($"/api/tracking/pages/{trackingPageId}/visits", new
        {
            occurredAtUtc = now,
            sessionId = "session-001",
            visitorFingerprint = "visitor-001",
            userAgent = "Mozilla/5.0",
            referrerUrl = "https://search.example",
            ipAddressHashPolicy = 2,
            deduplicationWindowSeconds = 180
        });

        visitOneResponse.EnsureSuccessStatusCode();

        var visitDuplicateResponse = await client.PostAsJsonAsync($"/api/tracking/pages/{trackingPageId}/visits", new
        {
            occurredAtUtc = now.AddSeconds(20),
            sessionId = "session-001",
            visitorFingerprint = "visitor-001",
            userAgent = "Mozilla/5.0",
            referrerUrl = "https://search.example",
            ipAddressHashPolicy = 2,
            deduplicationWindowSeconds = 180
        });

        visitDuplicateResponse.EnsureSuccessStatusCode();
        var duplicatePayload = JsonNode.Parse(await visitDuplicateResponse.Content.ReadAsStringAsync())!.AsObject();
        Assert.True(duplicatePayload["isDuplicate"]!.GetValue<bool>());

        var visitTwoResponse = await client.PostAsJsonAsync($"/api/tracking/pages/{trackingPageId}/visits", new
        {
            occurredAtUtc = now.AddMinutes(5),
            sessionId = "session-002",
            visitorFingerprint = "visitor-002",
            userAgent = "Mozilla/5.0",
            referrerUrl = "https://news.example",
            ipAddressHashPolicy = 2,
            deduplicationWindowSeconds = 60
        });

        visitTwoResponse.EnsureSuccessStatusCode();

        var analyticsResponse = await client.GetAsync($"/api/tracking/pages/{trackingPageId}/analytics?fromUtc={now.AddHours(-1):O}&toUtc={now.AddHours(1):O}&trendBucketSizeMinutes=60&recentVisitLimit=10");
        analyticsResponse.EnsureSuccessStatusCode();

        var analyticsPayload = JsonNode.Parse(await analyticsResponse.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal(2, analyticsPayload["summary"]!["totalVisits"]!.GetValue<int>());
        Assert.Equal(2, analyticsPayload["summary"]!["uniqueVisitors"]!.GetValue<int>());

        var overviewResponse = await client.GetAsync($"/api/tracking/analytics/overview?fromUtc={now.AddHours(-1):O}&toUtc={now.AddHours(1):O}&excludeBots=true&topPagesLimit=5&recentVisitLimit=10");
        overviewResponse.EnsureSuccessStatusCode();

        var overviewPayload = JsonNode.Parse(await overviewResponse.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal(2, overviewPayload["summary"]!["totalVisits"]!.GetValue<int>());
        Assert.NotEmpty(overviewPayload["metricDefinitions"]!.AsArray());
    }

    [Fact]
    public async Task TrackingEndpoints_ShouldEnforceRoleBasedAuthorization()
    {
        var viewerSlug = $"viewer-blocked-{UniqueSuffix}";
        var viewerClient = _factory.CreateClient();
        viewerClient.DefaultRequestHeaders.Add("X-Test-Role", "Viewer");

        var createAsViewer = await viewerClient.PostAsJsonAsync("/api/tracking/pages", new
        {
            slug = viewerSlug,
            title = "Viewer Blocked",
            destinationUrl = "https://example.com",
            ownerId = "viewer-1"
        });

        Assert.Equal(HttpStatusCode.Forbidden, createAsViewer.StatusCode);

        var operatorSlug = $"operator-page-{UniqueSuffix}";
        var operatorClient = _factory.CreateClient();
        operatorClient.DefaultRequestHeaders.Add("X-Test-Role", "Operator");

        var createAsOperator = await operatorClient.PostAsJsonAsync("/api/tracking/pages", new
        {
            slug = operatorSlug,
            title = "Operator Page",
            destinationUrl = "https://example.com/operator",
            ownerId = "operator-1"
        });

        createAsOperator.EnsureSuccessStatusCode();
        var created = JsonNode.Parse(await createAsOperator.Content.ReadAsStringAsync())!.AsObject();
        var trackingPageId = created["id"]!.GetValue<Guid>();

        var deleteAsOperator = await operatorClient.DeleteAsync($"/api/tracking/pages/{trackingPageId}");
        Assert.Equal(HttpStatusCode.Forbidden, deleteAsOperator.StatusCode);

        var adminClient = _factory.CreateClient();
        adminClient.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var deleteAsAdmin = await adminClient.DeleteAsync($"/api/tracking/pages/{trackingPageId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteAsAdmin.StatusCode);
    }

    [Fact]
    public async Task CampaignDelete_ShouldSoftDeleteLinkedTrackingPage()
    {
        var adminClient = _factory.CreateClient();
        adminClient.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        var (createCampaignResponse, _) = await CreateCampaignWithUniqueSlugAsync(
            adminClient,
            campaignNamePrefix: "Delete Cascade Campaign",
            trackingPageSlugPrefix: "delete-cascade-campaign",
            trackingPageTitle: "Delete Cascade Landing",
            trackingPageDescription: "Cascade soft-delete check",
            htmlContent: "<h1>Landing</h1>");

        createCampaignResponse.EnsureSuccessStatusCode();
        var createPayload = JsonNode.Parse(await createCampaignResponse.Content.ReadAsStringAsync())!.AsObject();
        var campaignId = createPayload["id"]!.GetValue<Guid>();
        var trackingPageId = createPayload["trackingPageId"]!.GetValue<Guid>();

        var deleteCampaignResponse = await adminClient.DeleteAsync($"/api/campaigns/{campaignId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteCampaignResponse.StatusCode);

        var campaignAfterDeleteResponse = await adminClient.GetAsync($"/api/campaigns/{campaignId}");
        Assert.Equal(HttpStatusCode.NotFound, campaignAfterDeleteResponse.StatusCode);

        var trackingPageAfterDeleteResponse = await adminClient.GetAsync($"/api/tracking/pages/{trackingPageId}");
        Assert.Equal(HttpStatusCode.NotFound, trackingPageAfterDeleteResponse.StatusCode);
    }
    
    [Fact]
    public async Task CampaignPublicLanding_ShouldRequireActiveLifecycleState()
    {
        var adminClient = _factory.CreateClient();
        adminClient.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        var (createCampaignResponse, slug) = await CreateCampaignWithUniqueSlugAsync(
            adminClient,
            campaignNamePrefix: "Lifecycle Guard Campaign",
            trackingPageSlugPrefix: "lifecycle-guard-campaign",
            trackingPageTitle: "Lifecycle Guard Landing",
            trackingPageDescription: "Only active campaigns can resolve publicly",
            htmlContent: "<h1>Lifecycle Guard</h1>");

        createCampaignResponse.EnsureSuccessStatusCode();
        var createPayload = JsonNode.Parse(await createCampaignResponse.Content.ReadAsStringAsync())!.AsObject();
        var campaignId = createPayload["id"]!.GetValue<Guid>();

        var publicWhileDraft = await adminClient.GetAsync($"/p/{slug}");
        Assert.Equal(HttpStatusCode.NotFound, publicWhileDraft.StatusCode);

        var startResponse = await adminClient.PostAsync($"/api/campaigns/{campaignId}/start", content: null);
        startResponse.EnsureSuccessStatusCode();

        var publicWhileActive = await adminClient.GetAsync($"/p/{slug}");
        publicWhileActive.EnsureSuccessStatusCode();

        var pauseResponse = await adminClient.PostAsync($"/api/campaigns/{campaignId}/pause", content: null);
        pauseResponse.EnsureSuccessStatusCode();

        var publicWhilePaused = await adminClient.GetAsync($"/p/{slug}");
        Assert.Equal(HttpStatusCode.NotFound, publicWhilePaused.StatusCode);
    }

    private static async Task<(HttpResponseMessage Response, string Slug)> CreateCampaignWithUniqueSlugAsync(
        HttpClient adminClient,
        string campaignNamePrefix,
        string trackingPageSlugPrefix,
        string trackingPageTitle,
        string trackingPageDescription,
        string htmlContent)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var suffix = UniqueSuffix;
            var slug = $"{trackingPageSlugPrefix}-{suffix}";
            var createCampaignResponse = await adminClient.PostAsJsonAsync("/api/campaigns", new
            {
                name = $"{campaignNamePrefix} {suffix}",
                trackingPageSlug = slug,
                trackingPageTitle,
                trackingPageDescription,
                templateId = (Guid?)null,
                htmlContent,
                validFromUtc = (DateTimeOffset?)null,
                validUntilUtc = (DateTimeOffset?)null
            });

            if (createCampaignResponse.StatusCode != HttpStatusCode.Conflict)
            {
                return (createCampaignResponse, slug);
            }
        }

        var fallbackSlug = $"{trackingPageSlugPrefix}-{UniqueSuffix}";
        var fallbackResponse = await adminClient.PostAsJsonAsync("/api/campaigns", new
        {
            name = $"{campaignNamePrefix} {UniqueSuffix}",
            trackingPageSlug = fallbackSlug,
            trackingPageTitle,
            trackingPageDescription,
            templateId = (Guid?)null,
            htmlContent,
            validFromUtc = (DateTimeOffset?)null,
            validUntilUtc = (DateTimeOffset?)null
        });

        return (fallbackResponse, fallbackSlug);
    }

    [Fact]
    public async Task LegacyPhishingEmailRoutes_ShouldReturnNotFound()
    {
        var client = _factory.CreateClient();

        var legacyTargetsRoute = await client.GetAsync("/api/campaigns/legacy-campaign/targets");
        var legacyEmailRoute = await client.GetAsync("/api/email/templates");

        Assert.Equal(HttpStatusCode.NotFound, legacyTargetsRoute.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, legacyEmailRoute.StatusCode);
    }
}
