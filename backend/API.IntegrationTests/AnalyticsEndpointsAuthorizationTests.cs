using System.Net;
using Xunit;

namespace QPhising.API.IntegrationTests;

public sealed class AnalyticsEndpointsAuthorizationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public AnalyticsEndpointsAuthorizationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Analytics_DashboardKpis_Should_Reject_Unauthenticated_Request()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/analytics/dashboard-kpis?from={Uri.EscapeDataString(DateTimeOffset.UtcNow.AddDays(-7).ToString("O"))}&to={Uri.EscapeDataString(DateTimeOffset.UtcNow.ToString("O"))}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Analytics_DashboardKpis_Should_Allow_Viewer_Role()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Test");
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.RolesHeader, AuthorizationPolicies.Viewer);

        string from = Uri.EscapeDataString(DateTimeOffset.UtcNow.AddDays(-7).ToString("O"));
        string to = Uri.EscapeDataString(DateTimeOffset.UtcNow.ToString("O"));

        var response = await client.GetAsync($"/api/analytics/dashboard-kpis?from={from}&to={to}&timeGrain=Day&timeZone=UTC");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Analytics_DashboardKpis_Should_Allow_Viewer_Role_On_Versioned_Route()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Test");
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.RolesHeader, AuthorizationPolicies.Viewer);

        string from = Uri.EscapeDataString(DateTimeOffset.UtcNow.AddDays(-7).ToString("O"));
        string to = Uri.EscapeDataString(DateTimeOffset.UtcNow.ToString("O"));

        var response = await client.GetAsync($"/api/v1/analytics/dashboard-kpis?from={from}&to={to}&timeGrain=Day&timeZone=UTC");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Analytics_DashboardKpis_Should_Reject_Request_With_Invalid_Range_Using_ProblemDetails()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Test");
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.RolesHeader, AuthorizationPolicies.Viewer);

        string from = Uri.EscapeDataString(DateTimeOffset.UtcNow.ToString("O"));
        string to = Uri.EscapeDataString(DateTimeOffset.UtcNow.AddDays(-1).ToString("O"));

        var response = await client.GetAsync($"/api/analytics/dashboard-kpis?from={from}&to={to}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
