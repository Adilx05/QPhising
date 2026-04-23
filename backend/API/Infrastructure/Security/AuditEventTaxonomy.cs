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

        if (method == "DELETE" && path.StartsWith("/api/campaigns/", StringComparison.OrdinalIgnoreCase))
        {
            return new AuditEventClassification("campaign.delete", path, IsSuccess(statusCode) ? "success" : "failure");
        }

        if (method == "POST" && path.Contains("/api/templates/", StringComparison.OrdinalIgnoreCase) && path.EndsWith("/publish", StringComparison.OrdinalIgnoreCase))
        {
            return new AuditEventClassification("template.publish", path, IsSuccess(statusCode) ? "success" : "failure");
        }

        return null;
    }

    private static bool IsSuccess(int statusCode) => statusCode is >= 200 and < 300;
}
