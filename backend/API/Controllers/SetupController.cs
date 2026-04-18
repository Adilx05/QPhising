using MediatR;
using Microsoft.AspNetCore.Mvc;
using QPhising.Api.Contracts.Setup;
using QPhising.Application.Contracts.Responses.Setup;
using QPhising.Application.CQRS.Commands.Setup;
using QPhising.Application.CQRS.Queries.Setup;

namespace QPhising.Api.Controllers;

[ApiController]
[Route("api/setup")]
[Produces("application/json", "application/problem+json")]
public sealed class SetupController : ControllerBase
{
    private readonly ISender _sender;

    public SetupController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("status", Name = "GetStatusSetup")]
    [ProducesResponseType(typeof(SetupStatusResult), StatusCodes.Status200OK)]
    public Task<SetupStatusResult> GetStatus(CancellationToken cancellationToken) =>
        _sender.Send(new GetSetupStatusQuery(), cancellationToken);

    [HttpGet("guard-decision", Name = "GetGuardDecisionSetup")]
    [ProducesResponseType(typeof(SetupGuardDecisionResult), StatusCodes.Status200OK)]
    public Task<SetupGuardDecisionResult> GetGuardDecision(CancellationToken cancellationToken) =>
        _sender.Send(new GetSetupGuardDecisionQuery(), cancellationToken);

    [HttpPost("test-db", Name = "TestDatabaseConnectionSetup")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(SetupDependencyTestResult), StatusCodes.Status200OK)]
    public Task<SetupDependencyTestResult> TestDatabaseConnection(
        [FromBody] TestDatabaseConnectionRequest request,
        CancellationToken cancellationToken) =>
        _sender.Send(new TestDatabaseConnectionCommand(request.ConnectionString), cancellationToken);

    [HttpPost("test-redis", Name = "TestRedisConnectionSetup")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(SetupDependencyTestResult), StatusCodes.Status200OK)]
    public Task<SetupDependencyTestResult> TestRedisConnection(
        [FromBody] TestRedisConnectionRequest request,
        CancellationToken cancellationToken) =>
        _sender.Send(new TestRedisConnectionCommand(request.ConnectionString), cancellationToken);

    [HttpPost("test-keycloak", Name = "TestKeycloakConnectionSetup")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(SetupDependencyTestResult), StatusCodes.Status200OK)]
    public Task<SetupDependencyTestResult> TestKeycloakConnection(
        [FromBody] TestKeycloakConnectionRequest request,
        CancellationToken cancellationToken) =>
        _sender.Send(
            new TestKeycloakConnectionCommand(request.Authority, request.Realm, request.ClientId, request.ClientSecret),
            cancellationToken);

    [HttpPost("save", Name = "SaveSetup")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(SetupStatusResult), StatusCodes.Status200OK)]
    public Task<SetupStatusResult> Save(
        [FromBody] SaveSetupConfigurationRequest request,
        CancellationToken cancellationToken) =>
        _sender.Send(
            new SaveSetupConfigurationCommand(
                request.DatabaseConnectionString,
                request.RedisConnectionString,
                request.KeycloakAuthority,
                request.KeycloakRealm,
                request.KeycloakClientId,
                request.KeycloakClientSecret),
            cancellationToken);
}
