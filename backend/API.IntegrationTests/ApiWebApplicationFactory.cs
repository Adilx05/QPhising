using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace QPhising.API.IntegrationTests;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.Scheme;
                options.DefaultChallengeScheme = TestAuthHandler.Scheme;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.Scheme, _ =>
            {
            });

            services.AddAuthorizationBuilder()
                .SetFallbackPolicy(new AuthorizationPolicyBuilder(TestAuthHandler.Scheme)
                    .RequireAuthenticatedUser()
                    .Build())
                .AddPolicy(AuthorizationPolicies.Admin, policy => policy.RequireRole(AuthorizationPolicies.Admin))
                .AddPolicy(AuthorizationPolicies.Operator, policy => policy.RequireRole(AuthorizationPolicies.Operator, AuthorizationPolicies.Admin))
                .AddPolicy(AuthorizationPolicies.Viewer, policy => policy.RequireRole(AuthorizationPolicies.Viewer, AuthorizationPolicies.Operator, AuthorizationPolicies.Admin));
        });
    }
}
