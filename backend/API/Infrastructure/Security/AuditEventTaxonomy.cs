namespace QPhising.Api.Infrastructure.Security;

public sealed record AuditEventClassification(string Action, string Resource, string Outcome);

public static class AuditEventTaxonomy
{
    public static AuditEventClassification? Classify(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var method = context.Request.Method.ToUpperInvariant();
        var statusCode = context.Response.StatusCode;

        if (statusCode is StatusCodes.Status401Unauthorized)
        {
            return new AuditEventClassification("security.unauthorized", path, "unauthorized");
        }

        if (statusCode is StatusCodes.Status403Forbidden)
        {
            return new AuditEventClassification("security.forbidden", path, "forbidden");
        }

        if (statusCode is StatusCodes.Status429TooManyRequests)
        {
            return new AuditEventClassification("security.rate_limited", path, "throttled");
        }

        var action = ClassifyAction(path, method);
        if (action is not null)
        {
            return new AuditEventClassification(action, path, IsSuccess(statusCode) ? "success" : "failure");
        }

        return null;
    }

    private static string? ClassifyAction(string path, string method)
    {
        if (method == "POST" && path.EndsWith("/start", StringComparison.OrdinalIgnoreCase) && path.StartsWith("/api/campaigns/", StringComparison.OrdinalIgnoreCase))
        {
            return "campaign.start";
        }

        if (method == "POST" && path.EndsWith("/pause", StringComparison.OrdinalIgnoreCase) && path.StartsWith("/api/campaigns/", StringComparison.OrdinalIgnoreCase))
        {
            return "campaign.pause";
        }

        if (method == "POST" && path.EndsWith("/complete", StringComparison.OrdinalIgnoreCase) && path.StartsWith("/api/campaigns/", StringComparison.OrdinalIgnoreCase))
        {
            return "campaign.complete";
        }

        if (method == "POST" && path.EndsWith("/cancel", StringComparison.OrdinalIgnoreCase) && path.StartsWith("/api/campaigns/", StringComparison.OrdinalIgnoreCase))
        {
            return "campaign.cancel";
        }

        if (method == "DELETE" && path.StartsWith("/api/campaigns/", StringComparison.OrdinalIgnoreCase))
        {
            return "campaign.delete";
        }

        if (method == "POST" && path == "/api/templates")
        {
            return "template.save";
        }

        if (method == "PUT" && path.StartsWith("/api/templates/", StringComparison.OrdinalIgnoreCase))
        {
            return "template.save";
        }

        if (method == "DELETE" && path.StartsWith("/api/templates/", StringComparison.OrdinalIgnoreCase))
        {
            return "template.delete";
        }

        if (method == "POST" && path.Contains("/api/tracking/pages/", StringComparison.OrdinalIgnoreCase) && path.EndsWith("/publish", StringComparison.OrdinalIgnoreCase))
        {
            return "tracking.publish";
        }

        if (method == "POST" && path.Contains("/api/tracking/pages/", StringComparison.OrdinalIgnoreCase) && path.EndsWith("/archive", StringComparison.OrdinalIgnoreCase))
        {
            return "tracking.archive";
        }

        if (method == "DELETE" && path.StartsWith("/api/tracking/pages/", StringComparison.OrdinalIgnoreCase))
        {
            return "tracking.delete";
        }

        if (method == "POST" && path.Contains("/api/templates/", StringComparison.OrdinalIgnoreCase) && path.EndsWith("/publish", StringComparison.OrdinalIgnoreCase))
        {
            return "template.publish";
        }

        return null;
    }

    private static bool IsSuccess(int statusCode) => statusCode is >= 200 and < 300;
}
