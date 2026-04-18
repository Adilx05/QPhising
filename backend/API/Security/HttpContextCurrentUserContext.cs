using System.Security.Claims;
using QPhising.Application.Contracts.Abstractions.Authorization;

namespace QPhising.Api.Security;

public sealed class HttpContextCurrentUserContext : ICurrentUserContext
{
    private static readonly string[] RoleClaimTypes =
    [
        ClaimTypes.Role,
        "role",
        "roles"
    ];

    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId =>
        Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? Principal?.FindFirstValue("sub");

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public IReadOnlyCollection<string> Roles => RoleClaimTypes
        .SelectMany(claimType => Principal?.FindAll(claimType) ?? Enumerable.Empty<Claim>())
        .Select(claim => claim.Value)
        .Where(role => !string.IsNullOrWhiteSpace(role))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;
}
