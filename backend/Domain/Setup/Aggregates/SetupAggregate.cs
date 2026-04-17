using System;

using QPhising.Domain.Setup.Enums;
using QPhising.Domain.Setup.ValueObjects;

namespace QPhising.Domain.Setup.Aggregates;

public sealed class SetupAggregate
{
    public bool IsSetupCompleted { get; private set; }

    public bool IsDatabaseConfigured { get; private set; }

    public bool IsKeycloakConfigured { get; private set; }

    public bool IsRedisConfigured { get; private set; }

    public SetupReadinessState ReadinessState { get; private set; } = SetupReadinessState.NotStarted;

    public SecureConfigValue? DatabaseConnectionCipherText { get; private set; }

    public SecureConfigValue? RedisConnectionCipherText { get; private set; }

    public string? KeycloakAuthority { get; private set; }

    public string? KeycloakRealm { get; private set; }

    public string? KeycloakClientId { get; private set; }

    public SecureConfigValue? KeycloakClientSecretCipherText { get; private set; }

    public void ConfigureDatabase(SecureConfigValue databaseConnectionCipherText)
    {
        ArgumentNullException.ThrowIfNull(databaseConnectionCipherText);

        DatabaseConnectionCipherText = databaseConnectionCipherText;
        IsDatabaseConfigured = true;

        UpdateReadinessState();
    }

    public void ConfigureRedis(SecureConfigValue redisConnectionCipherText)
    {
        ArgumentNullException.ThrowIfNull(redisConnectionCipherText);

        RedisConnectionCipherText = redisConnectionCipherText;
        IsRedisConfigured = true;

        UpdateReadinessState();
    }

    public void ConfigureKeycloak(
        Uri authority,
        string realm,
        string clientId,
        SecureConfigValue clientSecretCipherText)
    {
        ArgumentNullException.ThrowIfNull(authority);
        ArgumentNullException.ThrowIfNull(clientSecretCipherText);

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
        KeycloakClientSecretCipherText = clientSecretCipherText;
        IsKeycloakConfigured = true;

        UpdateReadinessState();
    }

    public void MarkSetupCompleted()
    {
        if (!IsFullyConfigured())
        {
            throw new InvalidOperationException("Setup cannot be completed until all required services are configured.");
        }

        IsSetupCompleted = true;
        ReadinessState = SetupReadinessState.Ready;
    }

    public bool IsFullyConfigured() => IsDatabaseConfigured && IsKeycloakConfigured && IsRedisConfigured;

    private void UpdateReadinessState()
    {
        if (IsSetupCompleted)
        {
            ReadinessState = SetupReadinessState.Ready;
            return;
        }

        if (!IsDatabaseConfigured && !IsKeycloakConfigured && !IsRedisConfigured)
        {
            ReadinessState = SetupReadinessState.NotStarted;
            return;
        }

        ReadinessState = SetupReadinessState.InProgress;
    }
}
