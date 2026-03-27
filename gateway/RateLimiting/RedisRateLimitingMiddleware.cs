using System.Globalization;
using System.Text.Json;
using Gateway.Configuration;
using Gateway.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Gateway.RateLimiting;

public sealed class RedisRateLimitingMiddleware(RequestDelegate next)
{
    private const string ClientIdHeader = "X-Client-Id";
    private const string LimitHeader = "X-RateLimit-Limit";
    private const string RemainingHeader = "X-RateLimit-Remaining";
    private const string ResetHeader = "X-RateLimit-Reset";
    private const string RetryAfterHeader = "Retry-After";

    private static readonly LuaScript IncrementRateLimitScript = LuaScript.Prepare(
        "local current = redis.call('INCR', KEYS[1]) " +
        "if current == 1 then redis.call('EXPIRE', KEYS[1], ARGV[1]) end " +
        "local ttl = redis.call('TTL', KEYS[1]) " +
        "return {current, ttl}");

    public async Task InvokeAsync(
        HttpContext context,
        IConnectionMultiplexer connectionMultiplexer,
        IOptions<RateLimitingOptions> options,
        ILogger<RedisRateLimitingMiddleware> logger)
    {
        var matchedRule = FindMatchedRule(context, options.Value);
        if (matchedRule is null)
        {
            await next(context);
            return;
        }

        context.Items[AccessLoggingContext.RateLimitAppliedKey] = true;
        context.Items[AccessLoggingContext.RateLimitRuleKey] = $"{matchedRule.PathPrefix}:{string.Join(",", matchedRule.Methods)}";

        var database = connectionMultiplexer.GetDatabase();
        var clientId = ResolveClientId(context);
        var normalizedPath = context.Request.Path.Value?.ToLowerInvariant() ?? "/";
        var windowSeconds = matchedRule.WindowSeconds <= 0 ? options.Value.DefaultWindowSeconds : matchedRule.WindowSeconds;
        var windowId = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / windowSeconds;
        var key = $"gateway:ratelimit:{clientId}:{context.Request.Method}:{normalizedPath}:{windowId}";

        RedisResult[] scriptResult;
        try
        {
            var evaluated = await database.ScriptEvaluateAsync(
                IncrementRateLimitScript,
                [new RedisKey(key)],
                [windowSeconds]).ConfigureAwait(false);

            scriptResult = (RedisResult[])evaluated!;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to evaluate rate limiting script for {Path}.", context.Request.Path);
            await WriteProblemResponseAsync(context, StatusCodes.Status503ServiceUnavailable, "Rate limiter unavailable.");
            return;
        }

        var currentCount = (int)scriptResult[0];
        var ttlSeconds = Math.Max((int)scriptResult[1], 1);
        var remaining = Math.Max(matchedRule.Limit - currentCount, 0);

        context.Response.Headers[LimitHeader] = matchedRule.Limit.ToString(CultureInfo.InvariantCulture);
        context.Response.Headers[RemainingHeader] = remaining.ToString(CultureInfo.InvariantCulture);
        context.Response.Headers[ResetHeader] = ttlSeconds.ToString(CultureInfo.InvariantCulture);

        if (currentCount > matchedRule.Limit)
        {
            context.Items[AccessLoggingContext.RateLimitExceededKey] = true;
            context.Response.Headers[RetryAfterHeader] = ttlSeconds.ToString(CultureInfo.InvariantCulture);
            logger.LogWarning(
                "Rate limit exceeded for client {ClientId} on {Method} {Path}. Limit={Limit} WindowSeconds={WindowSeconds}",
                clientId,
                context.Request.Method,
                context.Request.Path,
                matchedRule.Limit,
                windowSeconds);

            await WriteProblemResponseAsync(context, StatusCodes.Status429TooManyRequests, "Gateway rate limit exceeded.");
            return;
        }

        await next(context);
    }

    private static RateLimitRuleOptions? FindMatchedRule(HttpContext context, RateLimitingOptions options)
    {
        var requestPath = context.Request.Path.Value ?? string.Empty;
        var requestMethod = context.Request.Method;

        foreach (var rule in options.Rules)
        {
            if (!requestPath.StartsWith(rule.PathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (rule.Methods.Count == 0 || rule.Methods.Any(method => string.Equals(method, requestMethod, StringComparison.OrdinalIgnoreCase)))
            {
                return rule;
            }
        }

        return null;
    }

    private static string ResolveClientId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(ClientIdHeader, out var headerClientId) && !string.IsNullOrWhiteSpace(headerClientId))
        {
            return headerClientId.ToString().Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
    }

    private static async Task WriteProblemResponseAsync(HttpContext context, int statusCode, string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = "https://tools.ietf.org/html/rfc9110#section-15.5.9",
            title = statusCode == StatusCodes.Status429TooManyRequests ? "Too Many Requests" : "Service Unavailable",
            status = statusCode,
            detail,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
