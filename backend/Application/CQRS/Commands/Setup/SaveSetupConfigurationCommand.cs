using MediatR;
using QPhising.Application.Contracts.Responses.Setup;

namespace QPhising.Application.CQRS.Commands.Setup;

public sealed record SaveSetupConfigurationCommand(
    string DatabaseConnectionString,
    string? RedisConnectionString,
    string KeycloakAuthority,
    string KeycloakRealm,
    string KeycloakClientId,
    string KeycloakClientSecret) : IRequest<SetupStatusResult>;
