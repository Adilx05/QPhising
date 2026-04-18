using QPhising.Domain.RuntimeConfiguration.Aggregates;

namespace QPhising.Application.Contracts.Responses.RuntimeConfiguration;

public sealed record RuntimeConfigurationResult(
    bool IsDatabaseConfigured,
    bool IsRedisConfigured,
    bool IsKeycloakConfigured,
    bool IsReadyForProtectedRuntime,
    DateTimeOffset UpdatedAtUtc)
{
    public static RuntimeConfigurationResult FromAggregate(RuntimeConfigurationAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        return new RuntimeConfigurationResult(
            IsDatabaseConfigured: aggregate.DatabaseConnectionCipherText is not null,
            IsRedisConfigured: aggregate.RedisConnectionCipherText is not null,
            IsKeycloakConfigured:
                aggregate.KeycloakClientSecretCipherText is not null &&
                !string.IsNullOrWhiteSpace(aggregate.KeycloakAuthority) &&
                !string.IsNullOrWhiteSpace(aggregate.KeycloakRealm) &&
                !string.IsNullOrWhiteSpace(aggregate.KeycloakClientId),
            IsReadyForProtectedRuntime: aggregate.IsReadyForProtectedRuntime(),
            UpdatedAtUtc: aggregate.UpdatedAtUtc);
    }

    public static RuntimeConfigurationResult Empty() => new(
        IsDatabaseConfigured: false,
        IsRedisConfigured: false,
        IsKeycloakConfigured: false,
        IsReadyForProtectedRuntime: false,
        UpdatedAtUtc: DateTimeOffset.MinValue);
}
