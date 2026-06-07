using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using QPhising.Api.Tests.Infrastructure;
using Xunit;

namespace QPhising.Api.Tests;

public sealed class TemplateEndpointsIntegrationTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public TemplateEndpointsIntegrationTests(TestApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateTemplate_ShouldReturn200WithTemplateResult()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var response = await client.PostAsJsonAsync("/api/templates", new
        {
            name = "Create Test Template",
            htmlContent = "<html>Create Test</html>",
            description = "A test template created via integration test",
            tags = new[] { "test", "integration" }
        }, JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal("Create Test Template", payload["name"]!.GetValue<string>());
        Assert.Equal("<html>Create Test</html>", payload["htmlContent"]!.GetValue<string>());
        Assert.Equal("A test template created via integration test", payload["description"]!.GetValue<string>());
        Assert.NotEqual(Guid.Empty, payload["id"]!.GetValue<Guid>());
    }

    [Fact]
    public async Task ListTemplates_ShouldReturnTemplateArray()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        await client.PostAsJsonAsync("/api/templates", new
        {
            name = "List Test Template",
            htmlContent = "<html>List Test</html>",
            description = (string?)null,
            tags = (string[]?)null
        }, JsonOptions);

        var response = await client.GetAsync("/api/templates");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsArray();
        Assert.NotEmpty(payload);
    }

    [Fact]
    public async Task GetTemplateById_ShouldReturnTemplate()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var createResponse = await client.PostAsJsonAsync("/api/templates", new
        {
            name = "Get By Id Template",
            htmlContent = "<html>Get By Id</html>",
            description = "Template for get by id test",
            tags = new[] { "get-by-id" }
        }, JsonOptions);

        var created = JsonNode.Parse(await createResponse.Content.ReadAsStringAsync())!.AsObject();
        var templateId = created["id"]!.GetValue<Guid>();

        var response = await client.GetAsync($"/api/templates/{templateId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal(templateId, payload["id"]!.GetValue<Guid>());
        Assert.Equal("Get By Id Template", payload["name"]!.GetValue<string>());
    }

    [Fact]
    public async Task UpdateTemplate_ShouldReturnUpdatedTemplate()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var createResponse = await client.PostAsJsonAsync("/api/templates", new
        {
            name = "Original Template",
            htmlContent = "<html>Original</html>",
            description = "Original description",
            tags = new[] { "original" }
        }, JsonOptions);

        var created = JsonNode.Parse(await createResponse.Content.ReadAsStringAsync())!.AsObject();
        var templateId = created["id"]!.GetValue<Guid>();

        var response = await client.PutAsJsonAsync($"/api/templates/{templateId}", new
        {
            name = "Updated Template",
            htmlContent = "<html>Updated</html>",
            description = "Updated description",
            tags = new[] { "updated" }
        }, JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal("Updated Template", payload["name"]!.GetValue<string>());
        Assert.Equal("<html>Updated</html>", payload["htmlContent"]!.GetValue<string>());
        Assert.Equal(2, payload["version"]!.GetValue<int>());
    }

    [Fact]
    public async Task PublishTemplate_ShouldTransitionToPublished()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var createResponse = await client.PostAsJsonAsync("/api/templates", new
        {
            name = "Publish Test Template",
            htmlContent = "<html>Publish Test</html>",
            description = (string?)null,
            tags = (string[]?)null
        }, JsonOptions);

        var created = JsonNode.Parse(await createResponse.Content.ReadAsStringAsync())!.AsObject();
        var templateId = created["id"]!.GetValue<Guid>();

        var publishResponse = await client.PostAsync($"/api/templates/{templateId}/publish", null);

        Assert.Equal(HttpStatusCode.OK, publishResponse.StatusCode);
        var payload = JsonNode.Parse(await publishResponse.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal(1, payload["lifecycleState"]!.GetValue<int>());
    }

    [Fact]
    public async Task ArchiveTemplate_ShouldTransitionToArchived()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var createResponse = await client.PostAsJsonAsync("/api/templates", new
        {
            name = "Archive Test Template",
            htmlContent = "<html>Archive Test</html>",
            description = (string?)null,
            tags = (string[]?)null
        }, JsonOptions);

        var created = JsonNode.Parse(await createResponse.Content.ReadAsStringAsync())!.AsObject();
        var templateId = created["id"]!.GetValue<Guid>();

        var archiveResponse = await client.PostAsync($"/api/templates/{templateId}/archive", null);

        Assert.Equal(HttpStatusCode.OK, archiveResponse.StatusCode);
        var payload = JsonNode.Parse(await archiveResponse.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal(2, payload["lifecycleState"]!.GetValue<int>());
    }

    [Fact]
    public async Task DeleteTemplate_ShouldReturn204()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var createResponse = await client.PostAsJsonAsync("/api/templates", new
        {
            name = "Delete Test Template",
            htmlContent = "<html>Delete Test</html>",
            description = (string?)null,
            tags = (string[]?)null
        }, JsonOptions);

        var created = JsonNode.Parse(await createResponse.Content.ReadAsStringAsync())!.AsObject();
        var templateId = created["id"]!.GetValue<Guid>();

        var deleteResponse = await client.DeleteAsync($"/api/templates/{templateId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task RoleAuthorization_ViewerCannotCreate()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Viewer");

        var response = await client.PostAsJsonAsync("/api/templates", new
        {
            name = "Viewer Create Blocked",
            htmlContent = "<html>Viewer Blocked</html>",
            description = (string?)null,
            tags = (string[]?)null
        }, JsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RoleAuthorization_OperatorCanCreateButCannotDelete()
    {
        var operatorClient = _factory.CreateClient();
        operatorClient.DefaultRequestHeaders.Add("X-Test-Role", "Operator");

        var createResponse = await operatorClient.PostAsJsonAsync("/api/templates", new
        {
            name = "Operator Create Template",
            htmlContent = "<html>Operator Create</html>",
            description = "Created by operator",
            tags = new[] { "operator" }
        }, JsonOptions);

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var created = JsonNode.Parse(await createResponse.Content.ReadAsStringAsync())!.AsObject();
        var templateId = created["id"]!.GetValue<Guid>();

        var deleteAsOperator = await operatorClient.DeleteAsync($"/api/templates/{templateId}");
        Assert.Equal(HttpStatusCode.Forbidden, deleteAsOperator.StatusCode);

        var adminClient = _factory.CreateClient();
        adminClient.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        var deleteAsAdmin = await adminClient.DeleteAsync($"/api/templates/{templateId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteAsAdmin.StatusCode);
    }
}
