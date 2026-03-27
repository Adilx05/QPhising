using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPhising.Application.Features.Health;

namespace QPhising.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromServices] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetHealthQuery(), cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return Problem(title: "Unable to evaluate health status", statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok(result.Value);
    }
}
