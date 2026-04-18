using System.Security.Claims;
using QPhising.Gateway.Services;

namespace QPhising.Gateway.Middleware;

public sealed class ClaimsToHeadersForwardingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ConfigurationGatewayRoutePolicySettingsProvider _settingsProvider;

    public ClaimsToHeadersForwardingMiddleware(
        RequestDelegate next,
        ConfigurationGatewayRoutePolicySettingsProvider settingsProvider)
    {
        _next = next;
        _settingsProvider = settingsProvider;
    }

    public Task Invoke(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var settings = _settingsProvider.GetCurrent();

            foreach (var mapping in settings.ClaimsToHeaders)
            {
                var claimValue = ResolveClaimValue(context.User, mapping.Key);
                if (string.IsNullOrWhiteSpace(claimValue))
                {
                    continue;
                }

                context.Request.Headers[mapping.Value] = claimValue;
            }
        }

        return _next(context);
    }

    private static string? ResolveClaimValue(ClaimsPrincipal principal, string claimType)
    {
        if (claimType.Equals("realm_access.roles", StringComparison.OrdinalIgnoreCase))
        {
            var roleValues = principal.Claims
                .Where(claim => claim.Type is ClaimTypes.Role or "role" or "roles")
                .Select(claim => claim.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return roleValues.Length == 0 ? null : string.Join(",", roleValues);
        }

        return principal.FindFirst(claimType)?.Value;
    }
}
