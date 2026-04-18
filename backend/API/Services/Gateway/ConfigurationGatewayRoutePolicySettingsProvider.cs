using System.Collections.ObjectModel;
using QPhising.Application.Contracts.Abstractions.Gateway;
using QPhising.Application.Contracts.Responses.Gateway;

namespace QPhising.Api.Services.Gateway;

public sealed class ConfigurationGatewayRoutePolicySettingsProvider : IGatewayRoutePolicySettingsProvider
{
    private const string GatewayRoutePoliciesSectionPath = "Gateway:RoutePolicies";
    private const string AuthenticationProviderKeyPath = $"{GatewayRoutePoliciesSectionPath}:AuthenticationProviderKey";
    private const string ForwardAccessTokenPath = $"{GatewayRoutePoliciesSectionPath}:ForwardAccessToken";
    private const string ClaimsToHeadersPath = $"{GatewayRoutePoliciesSectionPath}:ClaimsToHeaders";

    private readonly IConfiguration _configuration;

    public ConfigurationGatewayRoutePolicySettingsProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<GatewayRoutePolicySettings> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var defaults = GatewayRoutePolicySettings.Default();
        var authenticationProviderKey = _configuration[AuthenticationProviderKeyPath];
        var forwardAccessToken = _configuration.GetValue<bool?>(ForwardAccessTokenPath);
        var configuredClaimsToHeaders = _configuration.GetSection(ClaimsToHeadersPath).Get<Dictionary<string, string>>();

        var claimsToHeaders = (configuredClaimsToHeaders is { Count: > 0 }
            ? configuredClaimsToHeaders
            : defaults.ClaimsToHeaders.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.OrdinalIgnoreCase));

        var settings = new GatewayRoutePolicySettings(
            AuthenticationProviderKey: string.IsNullOrWhiteSpace(authenticationProviderKey)
                ? defaults.AuthenticationProviderKey
                : authenticationProviderKey.Trim(),
            ClaimsToHeaders: new ReadOnlyDictionary<string, string>(claimsToHeaders),
            ForwardAccessToken: forwardAccessToken ?? defaults.ForwardAccessToken);

        return Task.FromResult(settings);
    }
}
