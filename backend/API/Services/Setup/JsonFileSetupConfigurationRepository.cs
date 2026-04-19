using System.Text.Json;
using System.Text.Json.Nodes;
using QPhising.Application.Contracts.Abstractions.Setup;
using QPhising.Domain.Setup.Aggregates;
using QPhising.Domain.Setup.ValueObjects;

namespace QPhising.Api.Services.Setup;

public sealed class JsonFileSetupConfigurationRepository : ISetupConfigurationRepository
{
    private const string SetupStateSectionName = "SetupWizardState";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly SemaphoreSlim FileGate = new(1, 1);

    private readonly string _runtimeSettingsPath;

    public JsonFileSetupConfigurationRepository(IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);
        _runtimeSettingsPath = Path.Combine(environment.ContentRootPath, "appsettings.runtime.json");
    }

    public async Task<SetupAggregate?> GetCurrentAsync(CancellationToken cancellationToken)
    {
        await FileGate.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(_runtimeSettingsPath))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(_runtimeSettingsPath, cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var rootNode = JsonNode.Parse(json) as JsonObject;
            var setupNode = rootNode?[SetupStateSectionName];
            if (setupNode is null)
            {
                return null;
            }

            var snapshot = setupNode.Deserialize<SetupConfigurationSnapshot>(SerializerOptions);
            return snapshot is null ? null : RehydrateAggregate(snapshot);
        }
        finally
        {
            FileGate.Release();
        }
    }

    public async Task SaveAsync(SetupAggregate aggregate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        await FileGate.WaitAsync(cancellationToken);
        try
        {
            JsonObject rootNode;
            if (File.Exists(_runtimeSettingsPath))
            {
                var existingJson = await File.ReadAllTextAsync(_runtimeSettingsPath, cancellationToken);
                rootNode = JsonNode.Parse(existingJson) as JsonObject ?? new JsonObject();
            }
            else
            {
                rootNode = new JsonObject();
            }

            rootNode[SetupStateSectionName] = JsonSerializer.SerializeToNode(ToSnapshot(aggregate), SerializerOptions);

            var directory = Path.GetDirectoryName(_runtimeSettingsPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var renderedJson = rootNode.ToJsonString(SerializerOptions);
            await File.WriteAllTextAsync(_runtimeSettingsPath, renderedJson, cancellationToken);
        }
        finally
        {
            FileGate.Release();
        }
    }

    private static SetupConfigurationSnapshot ToSnapshot(SetupAggregate aggregate)
    {
        if (aggregate.DatabaseConnectionCipherText is null ||
            aggregate.KeycloakClientSecretCipherText is null ||
            string.IsNullOrWhiteSpace(aggregate.KeycloakAuthority) ||
            string.IsNullOrWhiteSpace(aggregate.KeycloakRealm) ||
            string.IsNullOrWhiteSpace(aggregate.KeycloakClientId))
        {
            throw new InvalidOperationException("Setup configuration aggregate is incomplete and cannot be persisted.");
        }

        return new SetupConfigurationSnapshot(
            aggregate.DatabaseConnectionCipherText.CipherText,
            aggregate.RedisConnectionCipherText?.CipherText,
            aggregate.KeycloakAuthority,
            aggregate.KeycloakRealm,
            aggregate.KeycloakClientId,
            aggregate.KeycloakClientSecretCipherText.CipherText);
    }

    private static SetupAggregate RehydrateAggregate(SetupConfigurationSnapshot snapshot)
    {
        var aggregate = new SetupAggregate();

        aggregate.ConfigureDatabase(new SecureConfigValue(snapshot.DatabaseConnectionCipherText));
        if (!string.IsNullOrWhiteSpace(snapshot.RedisConnectionCipherText))
        {
            aggregate.ConfigureRedis(new SecureConfigValue(snapshot.RedisConnectionCipherText));
        }
        aggregate.ConfigureKeycloak(
            new Uri(snapshot.KeycloakAuthority, UriKind.Absolute),
            snapshot.KeycloakRealm,
            snapshot.KeycloakClientId,
            new SecureConfigValue(snapshot.KeycloakClientSecretCipherText));

        aggregate.MarkSetupCompleted();

        return aggregate;
    }

    private sealed record SetupConfigurationSnapshot(
        string DatabaseConnectionCipherText,
        string? RedisConnectionCipherText,
        string KeycloakAuthority,
        string KeycloakRealm,
        string KeycloakClientId,
        string KeycloakClientSecretCipherText);
}
