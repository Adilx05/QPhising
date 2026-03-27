using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QPhising.API.IntegrationTests;

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string Scheme = "Test";
    public const string RolesHeader = "X-Test-Roles";
    public const string UserIdHeader = "X-Test-UserId";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader) ||
            !string.Equals(authorizationHeader.ToString(), "Test", StringComparison.Ordinal))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        string userId = Request.Headers.TryGetValue(UserIdHeader, out var userIdHeader) &&
                        !string.IsNullOrWhiteSpace(userIdHeader)
            ? userIdHeader.ToString().Trim()
            : "integration-user";

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userId),
            new(ClaimTypes.NameIdentifier, userId),
            new("sub", userId)
        };
        if (Request.Headers.TryGetValue(RolesHeader, out var rolesHeader))
        {
            var roles = rolesHeader.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var identity = new ClaimsIdentity(claims, Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
