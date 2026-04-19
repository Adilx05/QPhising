namespace QPhising.Api.Contracts.Setup;

public sealed record SaveSetupConfigurationRequest(
    string DatabaseConnectionString,
    string? RedisConnectionString,
    string KeycloakAuthority,
    string KeycloakRealm,
    string KeycloakClientId,
    string KeycloakClientSecret);
