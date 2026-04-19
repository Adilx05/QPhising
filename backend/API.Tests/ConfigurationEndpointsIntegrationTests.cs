using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using QPhising.Api.Tests.Infrastructure;
using Xunit;

namespace QPhising.Api.Tests;

public sealed class ConfigurationEndpointsIntegrationTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;

    public ConfigurationEndpointsIntegrationTests(TestApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RuntimeConfigurationSave_ShouldAllowNullRedisAndReportReadyState()
    {
        var adminClient = _factory.CreateClient();
        adminClient.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var saveResponse = await adminClient.PostAsJsonAsync("/api/configuration", new
        {
            databaseConnectionString = "Host=localhost;Port=5432;Database=qphising;Username=qphising;Password=secret",
            redisConnectionString = (string?)null,
            keycloakAuthority = "https://keycloak.example.com/",
            keycloakRealm = "qphising",
            keycloakClientId = "qphising-api",
            keycloakClientSecret = "client-secret"
        });

        saveResponse.EnsureSuccessStatusCode();
        var payload = JsonNode.Parse(await saveResponse.Content.ReadAsStringAsync())!.AsObject();

        Assert.False(payload["isRedisConfigured"]!.GetValue<bool>());
        Assert.True(payload["isReadyForProtectedRuntime"]!.GetValue<bool>());
    }

    [Fact]
    public async Task RuntimeConfigurationEndpoints_ShouldEnforceRolePolicies()
    {
        var operatorClient = _factory.CreateClient();
        operatorClient.DefaultRequestHeaders.Add("X-Test-Role", "Operator");

        var saveAsOperator = await operatorClient.PostAsJsonAsync("/api/configuration", new
        {
            databaseConnectionString = "Host=localhost;Port=5432;Database=qphising;Username=qphising;Password=secret",
            redisConnectionString = (string?)null,
            keycloakAuthority = "https://keycloak.example.com/",
            keycloakRealm = "qphising",
            keycloakClientId = "qphising-api",
            keycloakClientSecret = "client-secret"
        });

        Assert.Equal(HttpStatusCode.Forbidden, saveAsOperator.StatusCode);

        var viewerClient = _factory.CreateClient();
        viewerClient.DefaultRequestHeaders.Add("X-Test-Role", "Viewer");

        var patchAsViewer = await viewerClient.PatchAsJsonAsync("/api/configuration", new
        {
            databaseConnectionString = "Host=localhost;Port=5432;Database=qphising-viewer;Username=qphising;Password=secret"
        });

        Assert.Equal(HttpStatusCode.Forbidden, patchAsViewer.StatusCode);

        var getAsViewer = await viewerClient.GetAsync("/api/configuration");
        getAsViewer.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task RuntimeConfigurationPatch_ShouldReturnProblemDetailsForInvalidKeycloakTuple()
    {
        var operatorClient = _factory.CreateClient();
        operatorClient.DefaultRequestHeaders.Add("X-Test-Role", "Operator");

        var patchResponse = await operatorClient.PatchAsJsonAsync("/api/configuration", new
        {
            keycloakAuthority = "https://keycloak.example.com/"
        });

        Assert.Equal(HttpStatusCode.BadRequest, patchResponse.StatusCode);

        var payload = JsonNode.Parse(await patchResponse.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal("One or more validation errors occurred.", payload["title"]!.GetValue<string>());
        Assert.NotNull(payload["errors"]);
    }
}
