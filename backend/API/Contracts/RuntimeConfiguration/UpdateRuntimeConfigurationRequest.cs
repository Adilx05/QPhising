namespace QPhising.Api.Contracts.RuntimeConfiguration;

public sealed record UpdateRuntimeConfigurationRequest(
    string? DatabaseConnectionString,
    string? RedisConnectionString,
    string? KeycloakAuthority,
    string? KeycloakRealm,
    string? KeycloakClientId,
    string? KeycloakClientSecret);
