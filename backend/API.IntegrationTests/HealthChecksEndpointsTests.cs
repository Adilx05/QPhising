using System.Net;
using System.Text.Json;
using Xunit;

namespace QPhising.API.IntegrationTests;

public sealed class HealthChecksEndpointsTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public HealthChecksEndpointsTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task Health_Endpoints_Should_Be_Anonymous_And_Return_Json(string endpoint)
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(endpoint);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        using var contentStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(contentStream);

        Assert.True(payload.RootElement.TryGetProperty("status", out _));
        Assert.True(payload.RootElement.TryGetProperty("traceId", out _));
        Assert.True(payload.RootElement.TryGetProperty("checks", out _));
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
    }
}
