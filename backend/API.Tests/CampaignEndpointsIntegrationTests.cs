using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using QPhising.Api.Tests.Infrastructure;
using Xunit;

namespace QPhising.Api.Tests;

public sealed class CampaignEndpointsIntegrationTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    private static string UniqueSuffix => Guid.NewGuid().ToString("N");

    public CampaignEndpointsIntegrationTests(TestApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateCampaign_ShouldReturn200WithCampaignResult()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        var slug = $"create-test-{UniqueSuffix}";

        var response = await client.PostAsJsonAsync("/api/campaigns", new
        {
            name = "Create Test Campaign",
            trackingPageSlug = slug,
            trackingPageTitle = "Create Test Title",
            trackingPageDescription = (string?)null,
            templateId = (Guid?)null,
            htmlContent = (string?)null,
            validFromUtc = (DateTimeOffset?)null,
            validUntilUtc = (DateTimeOffset?)null
        }, JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal("Create Test Campaign", payload["name"]!.GetValue<string>());
        Assert.NotEqual(Guid.Empty, payload["id"]!.GetValue<Guid>());
        Assert.NotEqual(Guid.Empty, payload["trackingPageId"]!.GetValue<Guid>());
    }

    [Fact]
    public async Task ListCampaigns_ShouldReturnCampaignArray()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        var slug = $"list-test-{UniqueSuffix}";

        await client.PostAsJsonAsync("/api/campaigns", new
        {
            name = "List Test Campaign",
            trackingPageSlug = slug,
            trackingPageTitle = "List Test Title",
            trackingPageDescription = (string?)null,
            templateId = (Guid?)null,
            htmlContent = (string?)null,
            validFromUtc = (DateTimeOffset?)null,
            validUntilUtc = (DateTimeOffset?)null
        }, JsonOptions);

        var response = await client.GetAsync("/api/campaigns");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsArray();
        Assert.NotEmpty(payload);
    }

    [Fact]
    public async Task GetCampaignById_ShouldReturnCampaign()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        var slug = $"getbyid-test-{UniqueSuffix}";

        var createResponse = await client.PostAsJsonAsync("/api/campaigns", new
        {
            name = "Get By Id Campaign",
            trackingPageSlug = slug,
            trackingPageTitle = "Get By Id Title",
            trackingPageDescription = (string?)null,
            templateId = (Guid?)null,
            htmlContent = (string?)null,
            validFromUtc = (DateTimeOffset?)null,
            validUntilUtc = (DateTimeOffset?)null
        }, JsonOptions);

        var created = JsonNode.Parse(await createResponse.Content.ReadAsStringAsync())!.AsObject();
        var campaignId = created["id"]!.GetValue<Guid>();

        var response = await client.GetAsync($"/api/campaigns/{campaignId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal(campaignId, payload["id"]!.GetValue<Guid>());
        Assert.Equal("Get By Id Campaign", payload["name"]!.GetValue<string>());
    }

    [Fact]
    public async Task UpdateCampaign_ShouldReturnUpdatedCampaign()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        var slug = $"update-test-{UniqueSuffix}";

        var createResponse = await client.PostAsJsonAsync("/api/campaigns", new
        {
            name = "Original Campaign",
            trackingPageSlug = slug,
            trackingPageTitle = "Update Title",
            trackingPageDescription = (string?)null,
            templateId = (Guid?)null,
            htmlContent = (string?)null,
            validFromUtc = (DateTimeOffset?)null,
            validUntilUtc = (DateTimeOffset?)null
        }, JsonOptions);

        var created = JsonNode.Parse(await createResponse.Content.ReadAsStringAsync())!.AsObject();
        var campaignId = created["id"]!.GetValue<Guid>();

        var response = await client.PutAsJsonAsync($"/api/campaigns/{campaignId}", new
        {
            name = "Updated Campaign",
            startsAtUtc = (DateTimeOffset?)null,
            endsAtUtc = (DateTimeOffset?)null
        }, JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal("Updated Campaign", payload["name"]!.GetValue<string>());
    }

    [Fact]
    public async Task DeleteCampaign_ShouldReturn204()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        var slug = $"delete-test-{UniqueSuffix}";

        var createResponse = await client.PostAsJsonAsync("/api/campaigns", new
        {
            name = "Delete Test Campaign",
            trackingPageSlug = slug,
            trackingPageTitle = "Delete Title",
            trackingPageDescription = (string?)null,
            templateId = (Guid?)null,
            htmlContent = (string?)null,
            validFromUtc = (DateTimeOffset?)null,
            validUntilUtc = (DateTimeOffset?)null
        }, JsonOptions);

        var created = JsonNode.Parse(await createResponse.Content.ReadAsStringAsync())!.AsObject();
        var campaignId = created["id"]!.GetValue<Guid>();

        var deleteResponse = await client.DeleteAsync($"/api/campaigns/{campaignId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task StartCampaign_ShouldTransitionToActive()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        var slug = $"start-test-{UniqueSuffix}";

        var createResponse = await client.PostAsJsonAsync("/api/campaigns", new
        {
            name = "Start Test Campaign",
            trackingPageSlug = slug,
            trackingPageTitle = "Start Title",
            trackingPageDescription = (string?)null,
            templateId = (Guid?)null,
            htmlContent = (string?)null,
            validFromUtc = (DateTimeOffset?)null,
            validUntilUtc = (DateTimeOffset?)null
        }, JsonOptions);

        var created = JsonNode.Parse(await createResponse.Content.ReadAsStringAsync())!.AsObject();
        var campaignId = created["id"]!.GetValue<Guid>();

        var startResponse = await client.PostAsync($"/api/campaigns/{campaignId}/start", null);

        Assert.Equal(HttpStatusCode.OK, startResponse.StatusCode);
        var payload = JsonNode.Parse(await startResponse.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal(2, payload["lifecycleState"]!.GetValue<int>());
    }

    [Fact]
    public async Task PauseCampaign_ShouldTransitionToPaused()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        var slug = $"pause-test-{UniqueSuffix}";

        var createResponse = await client.PostAsJsonAsync("/api/campaigns", new
        {
            name = "Pause Test Campaign",
            trackingPageSlug = slug,
            trackingPageTitle = "Pause Title",
            trackingPageDescription = (string?)null,
            templateId = (Guid?)null,
            htmlContent = (string?)null,
            validFromUtc = (DateTimeOffset?)null,
            validUntilUtc = (DateTimeOffset?)null
        }, JsonOptions);

        var created = JsonNode.Parse(await createResponse.Content.ReadAsStringAsync())!.AsObject();
        var campaignId = created["id"]!.GetValue<Guid>();

        await client.PostAsync($"/api/campaigns/{campaignId}/start", null);
        var pauseResponse = await client.PostAsync($"/api/campaigns/{campaignId}/pause", null);

        Assert.Equal(HttpStatusCode.OK, pauseResponse.StatusCode);
        var payload = JsonNode.Parse(await pauseResponse.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal(3, payload["lifecycleState"]!.GetValue<int>());
    }

    [Fact]
    public async Task CompleteCampaign_ShouldTransitionToCompleted()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        var slug = $"complete-test-{UniqueSuffix}";

        var createResponse = await client.PostAsJsonAsync("/api/campaigns", new
        {
            name = "Complete Test Campaign",
            trackingPageSlug = slug,
            trackingPageTitle = "Complete Title",
            trackingPageDescription = (string?)null,
            templateId = (Guid?)null,
            htmlContent = (string?)null,
            validFromUtc = (DateTimeOffset?)null,
            validUntilUtc = (DateTimeOffset?)null
        }, JsonOptions);

        var created = JsonNode.Parse(await createResponse.Content.ReadAsStringAsync())!.AsObject();
        var campaignId = created["id"]!.GetValue<Guid>();

        await client.PostAsync($"/api/campaigns/{campaignId}/start", null);
        var completeResponse = await client.PostAsync($"/api/campaigns/{campaignId}/complete", null);

        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);
        var payload = JsonNode.Parse(await completeResponse.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal(4, payload["lifecycleState"]!.GetValue<int>());
    }

    [Fact]
    public async Task CancelCampaign_ShouldTransitionToCancelled()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        var slug = $"cancel-test-{UniqueSuffix}";

        var createResponse = await client.PostAsJsonAsync("/api/campaigns", new
        {
            name = "Cancel Test Campaign",
            trackingPageSlug = slug,
            trackingPageTitle = "Cancel Title",
            trackingPageDescription = (string?)null,
            templateId = (Guid?)null,
            htmlContent = (string?)null,
            validFromUtc = (DateTimeOffset?)null,
            validUntilUtc = (DateTimeOffset?)null
        }, JsonOptions);

        var created = JsonNode.Parse(await createResponse.Content.ReadAsStringAsync())!.AsObject();
        var campaignId = created["id"]!.GetValue<Guid>();

        var cancelResponse = await client.PostAsync($"/api/campaigns/{campaignId}/cancel", null);

        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);
        var payload = JsonNode.Parse(await cancelResponse.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal(5, payload["lifecycleState"]!.GetValue<int>());
    }

    [Fact]
    public async Task ScheduleCampaign_ShouldTransitionToScheduled()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        var slug = $"schedule-test-{UniqueSuffix}";

        var createResponse = await client.PostAsJsonAsync("/api/campaigns", new
        {
            name = "Schedule Test Campaign",
            trackingPageSlug = slug,
            trackingPageTitle = "Schedule Title",
            trackingPageDescription = (string?)null,
            templateId = (Guid?)null,
            htmlContent = (string?)null,
            validFromUtc = (DateTimeOffset?)null,
            validUntilUtc = (DateTimeOffset?)null
        }, JsonOptions);

        var created = JsonNode.Parse(await createResponse.Content.ReadAsStringAsync())!.AsObject();
        var campaignId = created["id"]!.GetValue<Guid>();

        var scheduleResponse = await client.PostAsJsonAsync($"/api/campaigns/{campaignId}/schedule", new
        {
            startsAtUtc = DateTimeOffset.UtcNow.AddDays(1),
            endsAtUtc = DateTimeOffset.UtcNow.AddDays(2)
        }, JsonOptions);

        Assert.Equal(HttpStatusCode.OK, scheduleResponse.StatusCode);
        var payload = JsonNode.Parse(await scheduleResponse.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal(1, payload["lifecycleState"]!.GetValue<int>());
    }

    [Fact]
    public async Task RoleAuthorization_ViewerCannotCreate()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Viewer");

        var response = await client.PostAsJsonAsync("/api/campaigns", new
        {
            name = "Viewer Create Blocked",
            trackingPageSlug = $"viewer-create-{UniqueSuffix}",
            trackingPageTitle = "Viewer Blocked",
            trackingPageDescription = (string?)null,
            templateId = (Guid?)null,
            htmlContent = (string?)null,
            validFromUtc = (DateTimeOffset?)null,
            validUntilUtc = (DateTimeOffset?)null
        }, JsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RoleAuthorization_ViewerCannotStart()
    {
        var adminClient = _factory.CreateClient();
        adminClient.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        var slug = $"viewer-start-{UniqueSuffix}";

        var createResponse = await adminClient.PostAsJsonAsync("/api/campaigns", new
        {
            name = "Viewer Start Blocked",
            trackingPageSlug = slug,
            trackingPageTitle = "Viewer Start",
            trackingPageDescription = (string?)null,
            templateId = (Guid?)null,
            htmlContent = (string?)null,
            validFromUtc = (DateTimeOffset?)null,
            validUntilUtc = (DateTimeOffset?)null
        }, JsonOptions);

        var created = JsonNode.Parse(await createResponse.Content.ReadAsStringAsync())!.AsObject();
        var campaignId = created["id"]!.GetValue<Guid>();

        var viewerClient = _factory.CreateClient();
        viewerClient.DefaultRequestHeaders.Add("X-Test-Role", "Viewer");
        var startResponse = await viewerClient.PostAsync($"/api/campaigns/{campaignId}/start", null);

        Assert.Equal(HttpStatusCode.Forbidden, startResponse.StatusCode);
    }

    [Fact]
    public async Task RoleAuthorization_OperatorCanCreateButCannotDelete()
    {
        var operatorClient = _factory.CreateClient();
        operatorClient.DefaultRequestHeaders.Add("X-Test-Role", "Operator");
        var slug = $"op-test-{UniqueSuffix}";

        var createResponse = await operatorClient.PostAsJsonAsync("/api/campaigns", new
        {
            name = "Operator Create Campaign",
            trackingPageSlug = slug,
            trackingPageTitle = "Operator Create",
            trackingPageDescription = (string?)null,
            templateId = (Guid?)null,
            htmlContent = (string?)null,
            validFromUtc = (DateTimeOffset?)null,
            validUntilUtc = (DateTimeOffset?)null
        }, JsonOptions);

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var created = JsonNode.Parse(await createResponse.Content.ReadAsStringAsync())!.AsObject();
        var campaignId = created["id"]!.GetValue<Guid>();

        var deleteAsOperator = await operatorClient.DeleteAsync($"/api/campaigns/{campaignId}");
        Assert.Equal(HttpStatusCode.Forbidden, deleteAsOperator.StatusCode);

        var adminClient = _factory.CreateClient();
        adminClient.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        var deleteAsAdmin = await adminClient.DeleteAsync($"/api/campaigns/{campaignId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteAsAdmin.StatusCode);
    }
}
