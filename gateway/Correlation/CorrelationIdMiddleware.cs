using Serilog.Context;

namespace Gateway.Correlation;

public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public const string CorrelationIdHeader = "X-Correlation-ID";
    public const string CorrelationIdItemKey = "CorrelationId";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);

        context.Items[CorrelationIdItemKey] = correlationId;
        context.TraceIdentifier = correlationId;
        TrySetRequestCorrelationHeader(context, correlationId);
        context.Response.OnStarting(state =>
        {
            var httpContext = (HttpContext)state;
            TrySetResponseCorrelationHeader(httpContext, correlationId);
            return Task.CompletedTask;
        }, context);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            logger.LogDebug(
                "Correlation ID {CorrelationId} assigned for {Method} {Path}.",
                correlationId,
                context.Request.Method,
                context.Request.Path);

            await next(context);
        }
    }

    private void TrySetRequestCorrelationHeader(HttpContext context, string correlationId)
    {
        try
        {
            context.Request.Headers[CorrelationIdHeader] = correlationId;
        }
        catch (InvalidOperationException exception)
        {
            logger.LogWarning(
                exception,
                "Skipping correlation request header for {Method} {Path} because request headers are read-only.",
                context.Request.Method,
                context.Request.Path);
        }
    }

    private void TrySetResponseCorrelationHeader(HttpContext context, string correlationId)
    {
        if (context.Response.HasStarted)
        {
            logger.LogWarning(
                "Skipping correlation response header for {Method} {Path} because the response has already started.",
                context.Request.Method,
                context.Request.Path);
            return;
        }

        if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
        {
            try
            {
                context.Response.Headers[CorrelationIdHeader] = correlationId;
            }
            catch (InvalidOperationException exception)
            {
                logger.LogWarning(
                    exception,
                    "Skipping correlation response header for {Method} {Path} because headers are read-only.",
                    context.Request.Method,
                    context.Request.Path);
            }
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId)
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString().Trim();
        }

        return Guid.NewGuid().ToString("N");
    }
}
