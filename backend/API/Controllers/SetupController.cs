using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPhising.Application.Common;
using QPhising.Application.Features.Setup.ApplyMigrations;
using QPhising.Application.Features.Setup.FinalizeSetup;
using QPhising.Application.Features.Setup.GetSetupStatus;
using QPhising.Application.Features.Setup.ValidateDatabase;
using QPhising.Application.Features.Setup.ValidateSso;

namespace QPhising.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/setup")]
[Route("api/v{version:apiVersion}/setup")]
[Authorize(Policy = AuthorizationPolicies.Admin)]
public sealed class SetupController(IMediator mediator) : ControllerBase
{
    [HttpGet("status")]
    [ProducesResponseType(typeof(SetupStatusResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        Result<SetupStatusResponse> result = await mediator.Send(new GetSetupStatusQuery(), cancellationToken);
        return ToActionResult(result, "Unable to read setup status");
    }

    [HttpPost("validate-db")]
    [ProducesResponseType(typeof(ValidateDatabaseResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateDatabase([FromBody] ValidateDatabaseCommand command, CancellationToken cancellationToken)
    {
        Result<ValidateDatabaseResponse> result = await mediator.Send(command, cancellationToken);
        return ToActionResult(result, "Unable to validate database configuration");
    }

    [HttpPost("apply-migrations")]
    [ProducesResponseType(typeof(ApplyMigrationsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApplyMigrations([FromBody] ApplyMigrationsCommand command, CancellationToken cancellationToken)
    {
        Result<ApplyMigrationsResponse> result = await mediator.Send(command, cancellationToken);
        return ToActionResult(result, "Unable to apply database migrations");
    }

    [HttpPost("validate-sso")]
    [ProducesResponseType(typeof(ValidateSsoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateSso(CancellationToken cancellationToken)
    {
        Result<ValidateSsoResponse> result = await mediator.Send(new ValidateSsoCommand(), cancellationToken);
        return ToActionResult(result, "Unable to validate SSO configuration");
    }

    [HttpPost("finalize")]
    [ProducesResponseType(typeof(FinalizeSetupResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Finalize(CancellationToken cancellationToken)
    {
        Result<FinalizeSetupResponse> result = await mediator.Send(new FinalizeSetupCommand(), cancellationToken);
        return ToActionResult(result, "Unable to finalize setup");
    }

    private IActionResult ToActionResult<T>(Result<T> result, string title)
    {
        if (result.IsSuccess && result.Value is not null)
        {
            return Ok(result.Value);
        }

        return Problem(title: title, detail: string.Join("; ", result.Errors), statusCode: StatusCodes.Status400BadRequest);
    }
}
