# Persistence Aggregate Boundaries

This contract-source document defines persistence ownership for aggregate roots that require durable storage.

## SetupAggregate

- Storage kind: Relational
- Storage object: `platform.setup_configurations`
- Ownership: Setup module
- Required mapped fields:
  - `IsSetupCompleted` -> `is_setup_completed`
  - `DatabaseConnectionCipherText` -> `database_connection_cipher_text`
  - `RedisConnectionCipherText` -> `redis_connection_cipher_text`
  - `KeycloakAuthority` -> `keycloak_authority`
  - `KeycloakRealm` -> `keycloak_realm`
  - `KeycloakClientId` -> `keycloak_client_id`
  - `KeycloakClientSecretCipherText` -> `keycloak_client_secret_cipher_text`

## RuntimeConfigurationAggregate

- Storage kind: Relational
- Storage object: `platform.runtime_configurations`
- Ownership: RuntimeConfiguration module
- Required mapped fields:
  - `DatabaseConnectionCipherText` -> `database_connection_cipher_text`
  - `RedisConnectionCipherText` -> `redis_connection_cipher_text`
  - `KeycloakAuthority` -> `keycloak_authority`
  - `KeycloakRealm` -> `keycloak_realm`
  - `KeycloakClientId` -> `keycloak_client_id`
  - `KeycloakClientSecretCipherText` -> `keycloak_client_secret_cipher_text`
  - `UpdatedAtUtc` -> `updated_at_utc`

Reference implementation: `Domain/Persistence/Models/AggregatePersistenceMap.cs`.
