using System.Diagnostics;

namespace QPhising.Api.Middleware;

public sealed class CorrelationIdMiddleware
{
    public const string CorrelationIdHeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);

        context.Items[CorrelationIdHeaderName] = correlationId;
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object?>
               {
                   ["CorrelationId"] = correlationId,
                   ["TraceIdentifier"] = context.TraceIdentifier
               }))
        {
            await _next(context);
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var incomingCorrelationId) &&
            !string.IsNullOrWhiteSpace(incomingCorrelationId))
        {
            return incomingCorrelationId.ToString();
        }

        return Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
    }
}
