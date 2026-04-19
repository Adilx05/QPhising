namespace QPhising.Api.Contracts.RuntimeConfiguration;

public sealed record SaveRuntimeConfigurationRequest(
    string DatabaseConnectionString,
    string? RedisConnectionString,
    string KeycloakAuthority,
    string KeycloakRealm,
    string KeycloakClientId,
    string KeycloakClientSecret);
