using System.Net;
using Xunit;

namespace QPhising.API.IntegrationTests;

public sealed class AuthorizationFlowTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public AuthorizationFlowTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Admin_Endpoint_Should_Reject_Unauthenticated_Request()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/access/admin");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Admin_Endpoint_Should_Reject_Viewer_Role()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Test");
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.RolesHeader, AuthorizationPolicies.Viewer);

        var response = await client.GetAsync("/api/access/admin");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_Endpoint_Should_Allow_Admin_Role()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Test");
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.RolesHeader, AuthorizationPolicies.Admin);

        var response = await client.GetAsync("/api/access/admin");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Viewer_Endpoint_Should_Allow_Operator_Role()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Test");
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.RolesHeader, AuthorizationPolicies.Operator);

        var response = await client.GetAsync("/api/access/viewer");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
