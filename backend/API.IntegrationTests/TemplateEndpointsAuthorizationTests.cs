using System.Net;
using System.Net.Http.Json;
using QPhising.Domain.Templates;
using Xunit;

namespace QPhising.API.IntegrationTests;

public sealed class TemplateEndpointsAuthorizationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public TemplateEndpointsAuthorizationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Templates_List_Should_Reject_Unauthenticated_Request()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/templates");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Templates_Create_Should_Reject_Viewer_Role()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Test");
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.RolesHeader, AuthorizationPolicies.Viewer);

        var response = await client.PostAsJsonAsync("/api/templates", new
        {
            name = "Template",
            type = TemplateType.Email,
            htmlContent = "<p>{{first_name}}</p>",
            variables = new[] { "first_name" }
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Templates_Create_Should_Reject_Viewer_Role_On_Versioned_Route()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Test");
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.RolesHeader, AuthorizationPolicies.Viewer);

        var response = await client.PostAsJsonAsync("/api/v1/templates", new
        {
            name = "Template",
            type = TemplateType.Email,
            htmlContent = "<p>{{first_name}}</p>",
            variables = new[] { "first_name" }
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
