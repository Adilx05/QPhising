using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QPhising.API.Controllers;

[ApiController]
[Route("api/access")]
public sealed class AccessController : ControllerBase
{
    [HttpGet("admin")]
    [Authorize(Policy = AuthorizationPolicies.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Admin() => Ok(new { Area = AuthorizationPolicies.Admin, Allowed = true });

    [HttpGet("operator")]
    [Authorize(Policy = AuthorizationPolicies.Operator)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Operator() => Ok(new { Area = AuthorizationPolicies.Operator, Allowed = true });

    [HttpGet("viewer")]
    [Authorize(Policy = AuthorizationPolicies.Viewer)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Viewer() => Ok(new { Area = AuthorizationPolicies.Viewer, Allowed = true });
}
