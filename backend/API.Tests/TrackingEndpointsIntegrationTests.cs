using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using QPhising.Api.Tests.Infrastructure;
using Xunit;

namespace QPhising.Api.Tests;

public sealed class TrackingEndpointsIntegrationTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;

    public TrackingEndpointsIntegrationTests(TestApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TrackingCrud_VisitIngestion_AnalyticsOverview_ShouldWorkWithRepresentativeDataset()
    {
        var client = _factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/tracking/pages", new
        {
            slug = "quarterly-report",
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

        var publicLandingResponse = await client.GetAsync("/p/quarterly-report");
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
        var viewerClient = _factory.CreateClient();
        viewerClient.DefaultRequestHeaders.Add("X-Test-Role", "Viewer");

        var createAsViewer = await viewerClient.PostAsJsonAsync("/api/tracking/pages", new
        {
            slug = "viewer-blocked",
            title = "Viewer Blocked",
            destinationUrl = "https://example.com",
            ownerId = "viewer-1"
        });

        Assert.Equal(HttpStatusCode.Forbidden, createAsViewer.StatusCode);

        var operatorClient = _factory.CreateClient();
        operatorClient.DefaultRequestHeaders.Add("X-Test-Role", "Operator");

        var createAsOperator = await operatorClient.PostAsJsonAsync("/api/tracking/pages", new
        {
            slug = "operator-page",
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

        var createCampaignResponse = await adminClient.PostAsJsonAsync("/api/campaigns", new
        {
            name = "Delete Cascade Campaign",
            trackingPageSlug = "delete-cascade-campaign",
            trackingPageTitle = "Delete Cascade Landing",
            trackingPageDescription = "Cascade soft-delete check",
            templateId = (Guid?)null,
            htmlContent = "<h1>Landing</h1>",
            validFromUtc = (DateTimeOffset?)null,
            validUntilUtc = (DateTimeOffset?)null
        });

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
    public async Task LegacyPhishingEmailRoutes_ShouldReturnNotFound()
    {
        var client = _factory.CreateClient();

        var legacyTargetsRoute = await client.GetAsync("/api/campaigns/legacy-campaign/targets");
        var legacyEmailRoute = await client.GetAsync("/api/email/templates");

        Assert.Equal(HttpStatusCode.NotFound, legacyTargetsRoute.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, legacyEmailRoute.StatusCode);
    }
}
