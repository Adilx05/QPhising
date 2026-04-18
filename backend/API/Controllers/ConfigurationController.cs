using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPhising.Api.Contracts.RuntimeConfiguration;
using QPhising.Application.Contracts.Responses.RuntimeConfiguration;
using QPhising.Application.CQRS.Commands.RuntimeConfiguration;
using QPhising.Application.CQRS.Queries.RuntimeConfiguration;
using QPhising.Application.Security;

namespace QPhising.Api.Controllers;

[ApiController]
[Route("api/configuration")]
[Produces("application/json", "application/problem+json")]
[Authorize(Policy = IdentityAuthorizationPolicies.ViewerOrAbove)]
public sealed class ConfigurationController : ControllerBase
{
    private readonly ISender _sender;

    public ConfigurationController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet(Name = "Configuration_GetCurrent")]
    [ProducesResponseType(typeof(RuntimeConfigurationResult), StatusCodes.Status200OK)]
    public Task<RuntimeConfigurationResult> GetCurrent(CancellationToken cancellationToken) =>
        _sender.Send(new GetRuntimeConfigurationQuery(), cancellationToken);

    [HttpPost(Name = "Configuration_Save")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(RuntimeConfigurationResult), StatusCodes.Status200OK)]
    [Authorize(Policy = IdentityAuthorizationPolicies.AdminOnly)]
    public Task<RuntimeConfigurationResult> Save(
        [FromBody] SaveRuntimeConfigurationRequest request,
        CancellationToken cancellationToken) =>
        _sender.Send(
            new SaveRuntimeConfigurationCommand(
                request.DatabaseConnectionString,
                request.RedisConnectionString,
                request.KeycloakAuthority,
                request.KeycloakRealm,
                request.KeycloakClientId,
                request.KeycloakClientSecret),
            cancellationToken);

    [HttpPatch(Name = "Configuration_Update")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(RuntimeConfigurationResult), StatusCodes.Status200OK)]
    [Authorize(Policy = IdentityAuthorizationPolicies.OperatorOrAbove)]
    public Task<RuntimeConfigurationResult> Update(
        [FromBody] UpdateRuntimeConfigurationRequest request,
        CancellationToken cancellationToken) =>
        _sender.Send(
            new UpdateRuntimeConfigurationCommand(
                request.DatabaseConnectionString,
                request.RedisConnectionString,
                request.KeycloakAuthority,
                request.KeycloakRealm,
                request.KeycloakClientId,
                request.KeycloakClientSecret),
            cancellationToken);
}
