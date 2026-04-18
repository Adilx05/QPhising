using MediatR;
using Microsoft.AspNetCore.Mvc;
using QPhising.Api.Contracts.ProxyValidation;
using QPhising.Application.CQRS.Commands.ProxyValidation;

namespace QPhising.Api.Controllers;

[ApiController]
[Route("api/proxy-validation")]
public sealed class ProxyValidationController : ControllerBase
{
    private readonly ISender _sender;

    public ProxyValidationController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("assert-sync")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssertSync(
        [FromBody] AssertProxyContractSyncRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new AssertProxyContractSyncCommand(request.SwaggerContractPath, request.ProxyGenerationStampPath),
            cancellationToken);

        return NoContent();
    }
}
