using QPhising.Application.Contracts.Abstractions.Setup;

namespace QPhising.Api.Services.Setup;

public sealed class ConfigurationSetupBootstrapConfigurationReader : ISetupBootstrapConfigurationReader
{
    private readonly IConfiguration _configuration;

    public ConfigurationSetupBootstrapConfigurationReader(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool IsBootstrapConfigurationReady()
    {
        var databaseConnection = _configuration.GetConnectionString("DefaultConnection");
        var keycloakBaseUrl = _configuration["Keycloak:BaseUrl"];
        var keycloakRealm = _configuration["Keycloak:Realm"];
        var keycloakClientId = _configuration["Keycloak:ClientId"];
        var keycloakClientSecret = _configuration["Keycloak:ClientSecret"];

        return !string.IsNullOrWhiteSpace(databaseConnection)
            && !string.IsNullOrWhiteSpace(keycloakBaseUrl)
            && !string.IsNullOrWhiteSpace(keycloakRealm)
            && !string.IsNullOrWhiteSpace(keycloakClientId)
            && !string.IsNullOrWhiteSpace(keycloakClientSecret);
    }
}
