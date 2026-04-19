using Microsoft.AspNetCore.Http;

namespace QPhising.Api.Middleware;

public sealed class SecurityAuditMiddleware
{
    private readonly ILogger<SecurityAuditMiddleware> _logger;

    public SecurityAuditMiddleware(ILogger<SecurityAuditMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await next(context);

        if (context.Response.StatusCode is not (StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden or StatusCodes.Status429TooManyRequests))
        {
            return;
        }

        var endpoint = context.GetEndpoint()?.DisplayName ?? "unknown";
        var subject = context.User?.Identity?.Name ?? context.User?.FindFirst("sub")?.Value ?? "anonymous";
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        _logger.LogWarning(
            "Security audit event: status={StatusCode}, endpoint={Endpoint}, method={Method}, subject={Subject}, remoteIp={RemoteIp}.",
            context.Response.StatusCode,
            endpoint,
            context.Request.Method,
            subject,
            remoteIp);
    }
}
