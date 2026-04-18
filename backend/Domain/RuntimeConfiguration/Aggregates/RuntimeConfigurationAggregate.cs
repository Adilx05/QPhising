using System;

using QPhising.Domain.RuntimeConfiguration.Policies;
using QPhising.Domain.RuntimeConfiguration.ValueObjects;

namespace QPhising.Domain.RuntimeConfiguration.Aggregates;

public sealed class RuntimeConfigurationAggregate
{
    public RuntimeSecretValue? DatabaseConnectionCipherText { get; private set; }

    public RuntimeSecretValue? RedisConnectionCipherText { get; private set; }

    public string? KeycloakAuthority { get; private set; }

    public string? KeycloakRealm { get; private set; }

    public string? KeycloakClientId { get; private set; }

    public RuntimeSecretValue? KeycloakClientSecretCipherText { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; } = DateTimeOffset.MinValue;

    public bool HasAnyValueConfigured =>
        DatabaseConnectionCipherText is not null ||
        RedisConnectionCipherText is not null ||
        KeycloakClientSecretCipherText is not null ||
        !string.IsNullOrWhiteSpace(KeycloakAuthority) ||
        !string.IsNullOrWhiteSpace(KeycloakRealm) ||
        !string.IsNullOrWhiteSpace(KeycloakClientId);

    public void SetDatabaseConnection(string databaseConnectionCipherText)
    {
        DatabaseConnectionCipherText = RuntimeConfigurationSecretPolicy.RequireSecret(databaseConnectionCipherText, nameof(databaseConnectionCipherText));
        Touch();
    }

    public void SetRedisConnection(string redisConnectionCipherText)
    {
        RedisConnectionCipherText = RuntimeConfigurationSecretPolicy.RequireSecret(redisConnectionCipherText, nameof(redisConnectionCipherText));
        Touch();
    }

    public void SetKeycloakConfiguration(
        Uri authority,
        string realm,
        string clientId,
        string clientSecretCipherText)
    {
        ArgumentNullException.ThrowIfNull(authority);

        if (string.IsNullOrWhiteSpace(realm))
        {
            throw new ArgumentException("Keycloak realm is required.", nameof(realm));
        }

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentException("Keycloak client ID is required.", nameof(clientId));
        }

        KeycloakAuthority = authority.AbsoluteUri.TrimEnd('/');
        KeycloakRealm = realm.Trim();
        KeycloakClientId = clientId.Trim();
        KeycloakClientSecretCipherText = RuntimeConfigurationSecretPolicy.RequireSecret(clientSecretCipherText, nameof(clientSecretCipherText));

        Touch();
    }

    public bool IsReadyForProtectedRuntime() =>
        DatabaseConnectionCipherText is not null &&
        RedisConnectionCipherText is not null &&
        KeycloakClientSecretCipherText is not null &&
        !string.IsNullOrWhiteSpace(KeycloakAuthority) &&
        !string.IsNullOrWhiteSpace(KeycloakRealm) &&
        !string.IsNullOrWhiteSpace(KeycloakClientId);

    private void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;
}
