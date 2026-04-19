namespace QPhising.Api.Contracts.RuntimeConfiguration;

public sealed class SaveRuntimeConfigurationRequest
{
    public string DatabaseConnectionString { get; init; } = string.Empty;

    public string? RedisConnectionString { get; init; }

    public string KeycloakAuthority { get; init; } = string.Empty;

    public string KeycloakRealm { get; init; } = string.Empty;

    public string KeycloakClientId { get; init; } = string.Empty;

    public string KeycloakClientSecret { get; init; } = string.Empty;
}
