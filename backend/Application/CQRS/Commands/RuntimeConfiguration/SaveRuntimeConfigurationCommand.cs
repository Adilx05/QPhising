using MediatR;
using QPhising.Application.Contracts.Responses.RuntimeConfiguration;

namespace QPhising.Application.CQRS.Commands.RuntimeConfiguration;

public sealed record SaveRuntimeConfigurationCommand(
    string DatabaseConnectionString,
    string? RedisConnectionString,
    string KeycloakAuthority,
    string KeycloakRealm,
    string KeycloakClientId,
    string KeycloakClientSecret) : IRequest<RuntimeConfigurationResult>;
