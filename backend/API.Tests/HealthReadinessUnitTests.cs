using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using QPhising.Api.Health;
using Xunit;

namespace QPhising.Api.Tests;

public sealed class HealthReadinessUnitTests
{
    [Theory]
    [InlineData(HealthStatus.Healthy, "Healthy")]
    [InlineData(HealthStatus.Degraded, "Degraded")]
    [InlineData(HealthStatus.Unhealthy, "Unhealthy")]
    public void OperationalHealthStatus_ShouldMapKnownStatuses(HealthStatus status, string expected)
    {
        var result = OperationalHealthStatus.ToContractStatus(status);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task OperationalHealthResponseWriter_ShouldWriteSortedDependencies()
    {
        var report = new HealthReport(
            new Dictionary<string, HealthReportEntry>
            {
                ["zeta"] = new(HealthStatus.Degraded, "optional dependency down", TimeSpan.FromMilliseconds(8), null, null),
                ["alpha"] = new(HealthStatus.Healthy, "ok", TimeSpan.FromMilliseconds(1), null, null)
            },
            TimeSpan.FromMilliseconds(12));

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        await OperationalHealthResponseWriter.WriteAsync(context, report);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var payload = await JsonSerializer.DeserializeAsync<OperationalHealthResponse>(
            context.Response.Body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(payload);
        Assert.Equal("Degraded", payload!.OverallStatus);
        Assert.Collection(
            payload.Dependencies,
            dependency => Assert.Equal("alpha", dependency.Name),
            dependency => Assert.Equal("zeta", dependency.Name));
        Assert.Equal("optional dependency down", payload.Dependencies.Last().FailureReason);
    }

    [Fact]
    public async Task RedisOptionalHealthCheck_ShouldReturnDegraded_WhenDependencyDisabled()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HealthChecks:Redis:Enabled"] = "false"
            })
            .Build();

        var sut = new RedisOptionalHealthCheck(configuration);
        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Degraded, result.Status);
        Assert.Contains("optional dependency disabled", result.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task KeycloakReadinessHealthCheck_ShouldReturnDegraded_WhenProbeDisabled()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HealthChecks:Keycloak:Enabled"] = "false"
            })
            .Build();

        var sut = new KeycloakReadinessHealthCheck(new FakeHttpClientFactory(_ => throw new InvalidOperationException("Should not be called.")), configuration);

        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Degraded, result.Status);
        Assert.Contains("disabled", result.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task KeycloakReadinessHealthCheck_ShouldReturnUnhealthy_WhenAuthorityMissing()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var sut = new KeycloakReadinessHealthCheck(new FakeHttpClientFactory(_ => throw new InvalidOperationException("Should not be called.")), configuration);

        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Contains("Authority is missing", result.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task KeycloakReadinessHealthCheck_ShouldReturnHealthy_WhenMetadataAndTokenEndpointReachable()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Jwt:Authority"] = "https://identity.example.com/realms/qphising"
            })
            .Build();

        var sut = new KeycloakReadinessHealthCheck(
            new FakeHttpClientFactory(request =>
            {
                if (request.Method == HttpMethod.Get && request.RequestUri!.AbsolutePath.EndsWith("/.well-known/openid-configuration", StringComparison.Ordinal))
                {
                    var metadata = "{\"token_endpoint\":\"https://identity.example.com/realms/qphising/protocol/openid-connect/token\"}";
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(metadata, Encoding.UTF8, "application/json")
                    };
                }

                if (request.Method == HttpMethod.Head && request.RequestUri!.AbsolutePath.EndsWith("/protocol/openid-connect/token", StringComparison.Ordinal))
                {
                    return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }),
            configuration);

        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.Contains("reachable", result.Description, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FakeHttpClientFactory(Func<HttpRequestMessage, HttpResponseMessage> handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(new DelegatingStubHandler(handler));
    }

    private sealed class DelegatingStubHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(responseFactory(request));
    }
}
