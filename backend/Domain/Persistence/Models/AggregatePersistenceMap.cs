using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using QPhising.Domain.Persistence.Enums;

namespace QPhising.Domain.Persistence.Models;

public sealed class AggregatePersistenceMap
{
    private readonly List<AggregatePersistenceBoundary> _boundaries = [];

    public IReadOnlyCollection<AggregatePersistenceBoundary> Boundaries =>
        new ReadOnlyCollection<AggregatePersistenceBoundary>(_boundaries);

    public static AggregatePersistenceMap CreateDefault()
    {
        var map = new AggregatePersistenceMap();

        map.AddBoundary(new AggregatePersistenceBoundary(
            aggregateName: "SetupAggregate",
            storageKind: AggregateStorageKind.Relational,
            storageObject: "platform.setup_configurations",
            ownershipBoundary: "Setup module owns setup lifecycle and readiness persistence.",
            fields:
            [
                new AggregateFieldMapping("IsSetupCompleted", "is_setup_completed", isRequired: true, containsSecret: false),
                new AggregateFieldMapping("DatabaseConnectionCipherText", "database_connection_cipher_text", isRequired: true, containsSecret: true),
                new AggregateFieldMapping("RedisConnectionCipherText", "redis_connection_cipher_text", isRequired: true, containsSecret: true),
                new AggregateFieldMapping("KeycloakAuthority", "keycloak_authority", isRequired: true, containsSecret: false),
                new AggregateFieldMapping("KeycloakRealm", "keycloak_realm", isRequired: true, containsSecret: false),
                new AggregateFieldMapping("KeycloakClientId", "keycloak_client_id", isRequired: true, containsSecret: false),
                new AggregateFieldMapping("KeycloakClientSecretCipherText", "keycloak_client_secret_cipher_text", isRequired: true, containsSecret: true)
            ]));

        map.AddBoundary(new AggregatePersistenceBoundary(
            aggregateName: "RuntimeConfigurationAggregate",
            storageKind: AggregateStorageKind.Relational,
            storageObject: "platform.runtime_configurations",
            ownershipBoundary: "RuntimeConfiguration module owns runtime secrets and identity integration settings.",
            fields:
            [
                new AggregateFieldMapping("DatabaseConnectionCipherText", "database_connection_cipher_text", isRequired: true, containsSecret: true),
                new AggregateFieldMapping("RedisConnectionCipherText", "redis_connection_cipher_text", isRequired: true, containsSecret: true),
                new AggregateFieldMapping("KeycloakAuthority", "keycloak_authority", isRequired: true, containsSecret: false),
                new AggregateFieldMapping("KeycloakRealm", "keycloak_realm", isRequired: true, containsSecret: false),
                new AggregateFieldMapping("KeycloakClientId", "keycloak_client_id", isRequired: true, containsSecret: false),
                new AggregateFieldMapping("KeycloakClientSecretCipherText", "keycloak_client_secret_cipher_text", isRequired: true, containsSecret: true),
                new AggregateFieldMapping("UpdatedAtUtc", "updated_at_utc", isRequired: true, containsSecret: false)
            ]));

        return map;
    }

    public void AddBoundary(AggregatePersistenceBoundary boundary)
    {
        ArgumentNullException.ThrowIfNull(boundary);

        if (_boundaries.Any(existing => string.Equals(existing.AggregateName, boundary.AggregateName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Aggregate '{boundary.AggregateName}' already has a persistence boundary definition.");
        }

        _boundaries.Add(boundary);
    }
}
