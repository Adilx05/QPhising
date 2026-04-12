using System.Security.Claims;
using QPhising.Application.Common.Abstractions.Setup;

namespace QPhising.API.Setup;

public sealed class HttpSetupAuditContext(IHttpContextAccessor httpContextAccessor) : ISetupAuditContext
{
    public string GetActorIdentity()
    {
        ClaimsPrincipal? principal = httpContextAccessor.HttpContext?.User;
        if (principal is null)
        {
            return "anonymous";
        }

        return principal.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? principal.FindFirstValue("sub")
               ?? principal.Identity?.Name
               ?? "anonymous";
    }
}
