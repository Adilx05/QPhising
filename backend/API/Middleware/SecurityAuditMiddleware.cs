using System.Security.Cryptography;
using System.Text;
using QPhising.Api.Infrastructure.Persistence;
using QPhising.Api.Infrastructure.Persistence.Entities;
using QPhising.Api.Infrastructure.Security;

namespace QPhising.Api.Middleware;

public sealed class SecurityAuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityAuditMiddleware> _logger;

    public SecurityAuditMiddleware(RequestDelegate next, ILogger<SecurityAuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, QPhisingDbContext dbContext)
    {
        await _next(context);

        var classifiedEvent = AuditEventTaxonomy.Classify(context);
        if (classifiedEvent is null)
        {
            return;
        }

        var actor = context.User?.Identity?.Name
            ?? context.User?.FindFirst("sub")?.Value
            ?? "anonymous";

        var correlationId = context.Items.TryGetValue(CorrelationIdMiddleware.CorrelationIdHeaderName, out var correlationIdValue)
            ? correlationIdValue?.ToString() ?? context.TraceIdentifier
            : context.TraceIdentifier;

        var ipHash = ComputeIpHash(context.Connection.RemoteIpAddress?.ToString());

        var entity = new AuditLogEntryEntity
        {
            Id = Guid.NewGuid(),
            TimestampUtc = DateTimeOffset.UtcNow,
            Actor = actor,
            Action = classifiedEvent.Action,
            Resource = classifiedEvent.Resource,
            Outcome = classifiedEvent.Outcome,
            OutcomeCode = context.Response.StatusCode,
            CorrelationId = correlationId,
            IpHash = ipHash
        };

        dbContext.AuditLogEntries.Add(entity);

        try
        {
            await dbContext.SaveChangesAsync(context.RequestAborted);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to persist audit log event for {Action} at {Resource}.", entity.Action, entity.Resource);
            return;
        }

        _logger.LogInformation(
            "Audit event persisted: action={Action}, status={StatusCode}, endpoint={Endpoint}, actor={Actor}, correlationId={CorrelationId}.",
            entity.Action,
            entity.OutcomeCode,
            entity.Resource,
            entity.Actor,
            entity.CorrelationId);
    }

    private static string? ComputeIpHash(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return null;
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(ipAddress));
        return Convert.ToHexString(bytes);
    }
}
