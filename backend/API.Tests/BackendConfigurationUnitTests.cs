using QPhising.Application.Contracts.Abstractions.RuntimeConfiguration;
using QPhising.Application.CQRS.Commands.RuntimeConfiguration;
using QPhising.Application.CQRS.Commands.Setup;
using QPhising.Domain.RuntimeConfiguration.Aggregates;
using Xunit;

namespace QPhising.Api.Tests;

public sealed class BackendConfigurationUnitTests
{
    [Fact]
    public void SaveSetupConfigurationCommandValidator_ShouldAllowOptionalRedis()
    {
        var validator = new SaveSetupConfigurationCommandValidator();

        var result = validator.Validate(new SaveSetupConfigurationCommand(
            DatabaseConnectionString: "Host=localhost;Port=5432;Database=qphising;Username=qphising;Password=secret",
            RedisConnectionString: null,
            KeycloakAuthority: "https://keycloak.example.com",
            KeycloakRealm: "qphising",
            KeycloakClientId: "qphising-api",
            KeycloakClientSecret: "super-secret"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateRuntimeConfigurationCommandValidator_ShouldRequireFullKeycloakTupleWhenUpdatingAuthority()
    {
        var validator = new UpdateRuntimeConfigurationCommandValidator();

        var result = validator.Validate(new UpdateRuntimeConfigurationCommand(
            DatabaseConnectionString: null,
            RedisConnectionString: null,
            KeycloakAuthority: "https://keycloak.example.com",
            KeycloakRealm: null,
            KeycloakClientId: null,
            KeycloakClientSecret: null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("provided together", StringComparison.Ordinal));
    }

    [Fact]
    public async Task UpdateRuntimeConfigurationCommandHandler_ShouldPreserveExistingKeycloakWhenOnlyDatabaseChanges()
    {
        var existing = new RuntimeConfigurationAggregate();
        existing.SetDatabaseConnection("cipher-db-old");
        existing.SetRedisConnection("cipher-redis-old");
        existing.SetKeycloakConfiguration(
            new Uri("https://keycloak.old.example.com"),
            "old-realm",
            "old-client",
            "cipher-secret-old");

        var repository = new FakeRuntimeConfigurationRepository(existing);
        var cipher = new FakeRuntimeConfigurationSecretCipher();
        var handler = new UpdateRuntimeConfigurationCommandHandler(repository, cipher);

        var result = await handler.Handle(
            new UpdateRuntimeConfigurationCommand(
                DatabaseConnectionString: "Host=localhost;Port=5432;Database=qphising;Username=qphising;Password=secret",
                RedisConnectionString: null,
                KeycloakAuthority: null,
                KeycloakRealm: null,
                KeycloakClientId: null,
                KeycloakClientSecret: null),
            CancellationToken.None);

        Assert.True(result.IsReadyForProtectedRuntime);
        Assert.True(result.IsKeycloakConfigured);
        Assert.True(result.IsRedisConfigured);

        var persisted = await repository.GetCurrentAsync(CancellationToken.None);
        Assert.NotNull(persisted);
        Assert.Equal("https://keycloak.old.example.com", persisted!.KeycloakAuthority);
        Assert.Equal("old-realm", persisted.KeycloakRealm);
        Assert.Equal("old-client", persisted.KeycloakClientId);
    }

    private sealed class FakeRuntimeConfigurationRepository : IRuntimeConfigurationRepository
    {
        private RuntimeConfigurationAggregate? _current;

        public FakeRuntimeConfigurationRepository(RuntimeConfigurationAggregate current)
        {
            _current = current;
        }

        public Task<RuntimeConfigurationAggregate?> GetCurrentAsync(CancellationToken cancellationToken)
            => Task.FromResult(_current);

        public Task SaveAsync(RuntimeConfigurationAggregate aggregate, CancellationToken cancellationToken)
        {
            _current = aggregate;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeRuntimeConfigurationSecretCipher : IRuntimeConfigurationSecretCipher
    {
        public Task<string> EncryptAsync(string value, CancellationToken cancellationToken)
            => Task.FromResult($"enc::{value}");

        public Task<string> DecryptAsync(string cipherText, CancellationToken cancellationToken)
            => Task.FromResult(cipherText.Replace("enc::", string.Empty, StringComparison.Ordinal));
    }
}
