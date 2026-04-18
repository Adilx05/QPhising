using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPhising.Application.Contracts.Responses.Gateway;
using QPhising.Application.CQRS.Queries.Gateway;
using QPhising.Application.Security;

namespace QPhising.Api.Controllers;

[ApiController]
[Route("api/gateway")]
[Produces("application/json", "application/problem+json")]
[Authorize(Policy = IdentityAuthorizationPolicies.AdminOnly)]
public sealed class GatewayController : ControllerBase
{
    private readonly ISender _sender;

    public GatewayController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("route-policies", Name = "GetGatewayRoutePolicies")]
    [ProducesResponseType(typeof(GatewayRoutePolicyCompositionResult), StatusCodes.Status200OK)]
    public Task<GatewayRoutePolicyCompositionResult> GetRoutePolicies(CancellationToken cancellationToken) =>
        _sender.Send(new ComposeGatewayRoutePoliciesQuery(), cancellationToken);
}
