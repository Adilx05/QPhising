using MediatR;
using Microsoft.AspNetCore.Mvc;
using QPhising.Api.Contracts.RuntimeConfiguration;
using QPhising.Application.Contracts.Responses.RuntimeConfiguration;
using QPhising.Application.CQRS.Commands.RuntimeConfiguration;
using QPhising.Application.CQRS.Queries.RuntimeConfiguration;

namespace QPhising.Api.Controllers;

[ApiController]
[Route("api/configuration")]
[Produces("application/json", "application/problem+json")]
public sealed class ConfigurationController : ControllerBase
{
    private readonly ISender _sender;

    public ConfigurationController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet(Name = "GetRuntimeConfiguration")]
    [ProducesResponseType(typeof(RuntimeConfigurationResult), StatusCodes.Status200OK)]
    public Task<RuntimeConfigurationResult> GetCurrent(CancellationToken cancellationToken) =>
        _sender.Send(new GetRuntimeConfigurationQuery(), cancellationToken);

    [HttpPost(Name = "SaveRuntimeConfiguration")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(RuntimeConfigurationResult), StatusCodes.Status200OK)]
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

    [HttpPatch(Name = "UpdateRuntimeConfiguration")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(RuntimeConfigurationResult), StatusCodes.Status200OK)]
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
