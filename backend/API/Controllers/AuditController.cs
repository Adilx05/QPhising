using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPhising.Api.Contracts.Audit;
using QPhising.Application.Contracts.Responses.Audit;
using QPhising.Application.CQRS.Queries.Audit;
using QPhising.Application.Security;

namespace QPhising.Api.Controllers;

[ApiController]
[Route("api/audit/logs")]
[Produces("application/json", "application/problem+json")]
[Authorize(Policy = IdentityAuthorizationPolicies.OperatorOrAbove)]
public sealed class AuditController : ControllerBase
{
    private readonly ISender _sender;

    public AuditController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet(Name = "Audit_ListLogs")]
    [ProducesResponseType(typeof(AuditLogPageResult), StatusCodes.Status200OK)]
    public Task<AuditLogPageResult> List([FromQuery] ListAuditLogsRequest request, CancellationToken cancellationToken) =>
        _sender.Send(
            new ListAuditLogsQuery(
                request.FromUtc,
                request.ToUtc,
                request.Actor,
                request.Endpoint,
                request.OutcomeCode,
                request.CorrelationId,
                request.Page,
                request.PageSize,
                request.SortBy,
                request.SortDirection),
            cancellationToken);
}
