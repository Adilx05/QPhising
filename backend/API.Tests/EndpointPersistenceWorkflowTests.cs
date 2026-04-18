using System.Net.Http.Json;
using System.Text.Json.Nodes;
using QPhising.Api.Tests.Infrastructure;
using Xunit;

namespace QPhising.Api.Tests;

public sealed class EndpointPersistenceWorkflowTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;

    public EndpointPersistenceWorkflowTests(TestApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SetupSave_ThenStatus_ShouldReturnReadyStateFromPersistedWorkflow()
    {
        var client = _factory.CreateClient();

        var saveResponse = await client.PostAsJsonAsync("/api/setup/save", new
        {
            databaseConnectionString = "Host=localhost;Port=5432;Database=qphising;Username=qphising;Password=secret",
            redisConnectionString = "localhost:6379,password=secret",
            keycloakAuthority = "https://keycloak.example.com/",
            keycloakRealm = "qphising",
            keycloakClientId = "qphising-api",
            keycloakClientSecret = "client-secret"
        });

        saveResponse.EnsureSuccessStatusCode();

        var statusResponse = await client.GetAsync("/api/setup/status");
        statusResponse.EnsureSuccessStatusCode();

        var statusPayload = JsonNode.Parse(await statusResponse.Content.ReadAsStringAsync())!.AsObject();

        Assert.True(statusPayload["isDatabaseConfigured"]!.GetValue<bool>());
        Assert.True(statusPayload["isRedisConfigured"]!.GetValue<bool>());
        Assert.True(statusPayload["isKeycloakConfigured"]!.GetValue<bool>());
        Assert.Equal(2, statusPayload["readinessState"]!.GetValue<int>());
    }

    [Fact]
    public async Task RuntimeSave_ThenGet_ShouldReturnReadyRuntimeConfigurationFromPersistedWorkflow()
    {
        var client = _factory.CreateClient();

        var saveResponse = await client.PostAsJsonAsync("/api/configuration", new
        {
            databaseConnectionString = "Host=localhost;Port=5432;Database=qphising;Username=qphising;Password=secret",
            redisConnectionString = "localhost:6379,password=secret",
            keycloakAuthority = "https://keycloak.example.com/",
            keycloakRealm = "qphising",
            keycloakClientId = "qphising-api",
            keycloakClientSecret = "client-secret"
        });

        saveResponse.EnsureSuccessStatusCode();

        var getResponse = await client.GetAsync("/api/configuration");
        getResponse.EnsureSuccessStatusCode();

        var payload = JsonNode.Parse(await getResponse.Content.ReadAsStringAsync())!.AsObject();

        Assert.True(payload["isDatabaseConfigured"]!.GetValue<bool>());
        Assert.True(payload["isRedisConfigured"]!.GetValue<bool>());
        Assert.True(payload["isKeycloakConfigured"]!.GetValue<bool>());
        Assert.True(payload["isReadyForProtectedRuntime"]!.GetValue<bool>());
        Assert.NotEqual(DateTimeOffset.MinValue, payload["updatedAtUtc"]!.GetValue<DateTimeOffset>());
    }
}
