using System.Diagnostics;
using System.Security.Claims;
using Gateway.Correlation;

namespace Gateway.Logging;

public sealed class AccessLoggingMiddleware(RequestDelegate next, ILogger<AccessLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;
            var isAuthenticated = context.User.Identity?.IsAuthenticated == true;
            var principal = ResolvePrincipal(context.User);
            var route = context.Request.Path.Value ?? "/";
            var correlationId = ResolveContextString(context, CorrelationIdMiddleware.CorrelationIdItemKey)
                ?? context.TraceIdentifier;

            var rateLimitApplied = ResolveContextBool(context, AccessLoggingContext.RateLimitAppliedKey);
            var rateLimitExceeded = ResolveContextBool(context, AccessLoggingContext.RateLimitExceededKey) || statusCode == StatusCodes.Status429TooManyRequests;
            var rateLimitRule = ResolveContextString(context, AccessLoggingContext.RateLimitRuleKey) ?? "none";

            var securityOutcome = statusCode switch
            {
                StatusCodes.Status401Unauthorized => "unauthenticated",
                StatusCodes.Status403Forbidden => "forbidden",
                _ when isAuthenticated => "authorized",
                _ => "anonymous"
            };

            var logLevel = statusCode switch
            {
                >= 500 => LogLevel.Error,
                StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden or StatusCodes.Status429TooManyRequests => LogLevel.Warning,
                _ => LogLevel.Information
            };

            logger.Log(
                logLevel,
                "Gateway access {Method} {Route} responded {StatusCode} in {LatencyMs}ms (principal={Principal}, auth={IsAuthenticated}, securityOutcome={SecurityOutcome}, throttled={RateLimitExceeded}, rateLimitApplied={RateLimitApplied}, rateLimitRule={RateLimitRule}, correlationId={CorrelationId}, startedAt={StartedAtUtc})",
                context.Request.Method,
                route,
                statusCode,
                stopwatch.ElapsedMilliseconds,
                principal,
                isAuthenticated,
                securityOutcome,
                rateLimitExceeded,
                rateLimitApplied,
                rateLimitRule,
                correlationId,
                startedAt);
        }
    }

    private static string ResolvePrincipal(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return "anonymous";
        }

        return principal.FindFirst("preferred_username")?.Value
            ?? principal.FindFirst(ClaimTypes.Name)?.Value
            ?? principal.FindFirst("sub")?.Value
            ?? principal.Identity?.Name
            ?? "authenticated";
    }

    private static bool ResolveContextBool(HttpContext context, string key)
    {
        if (!context.Items.TryGetValue(key, out var value) || value is null)
        {
            return false;
        }

        return value switch
        {
            bool boolValue => boolValue,
            string stringValue when bool.TryParse(stringValue, out var parsedValue) => parsedValue,
            _ => false
        };
    }

    private static string? ResolveContextString(HttpContext context, string key)
    {
        return context.Items.TryGetValue(key, out var value) ? value?.ToString() : null;
    }
}
