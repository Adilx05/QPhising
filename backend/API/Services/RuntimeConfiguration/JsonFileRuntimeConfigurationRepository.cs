using System.Text.Json;
using System.Text.Json.Nodes;
using QPhising.Application.Contracts.Abstractions.RuntimeConfiguration;
using QPhising.Domain.RuntimeConfiguration.Aggregates;

namespace QPhising.Api.Services.RuntimeConfiguration;

public sealed class JsonFileRuntimeConfigurationRepository : IRuntimeConfigurationRepository
{
    private const string RuntimeConfigurationSectionName = "RuntimeConfigurationState";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly SemaphoreSlim FileGate = new(1, 1);

    private readonly string _runtimeSettingsPath;

    public JsonFileRuntimeConfigurationRepository(IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);
        _runtimeSettingsPath = Path.Combine(environment.ContentRootPath, "appsettings.runtime.json");
    }

    public async Task<RuntimeConfigurationAggregate?> GetCurrentAsync(CancellationToken cancellationToken)
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
            var runtimeConfigurationNode = rootNode?[RuntimeConfigurationSectionName];
            if (runtimeConfigurationNode is null)
            {
                return null;
            }

            var snapshot = runtimeConfigurationNode.Deserialize<RuntimeConfigurationSnapshot>(SerializerOptions);
            return snapshot is null ? null : FromSnapshot(snapshot);
        }
        finally
        {
            FileGate.Release();
        }
    }

    public async Task SaveAsync(RuntimeConfigurationAggregate aggregate, CancellationToken cancellationToken)
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

            rootNode[RuntimeConfigurationSectionName] = JsonSerializer.SerializeToNode(ToSnapshot(aggregate), SerializerOptions);

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

    private static RuntimeConfigurationSnapshot ToSnapshot(RuntimeConfigurationAggregate aggregate)
    {
        return new RuntimeConfigurationSnapshot(
            aggregate.DatabaseConnectionCipherText?.CipherText,
            aggregate.RedisConnectionCipherText?.CipherText,
            aggregate.KeycloakAuthority,
            aggregate.KeycloakRealm,
            aggregate.KeycloakClientId,
            aggregate.KeycloakClientSecretCipherText?.CipherText,
            aggregate.UpdatedAtUtc);
    }

    private static RuntimeConfigurationAggregate? FromSnapshot(RuntimeConfigurationSnapshot snapshot)
    {
        var aggregate = new RuntimeConfigurationAggregate();

        if (!string.IsNullOrWhiteSpace(snapshot.DatabaseConnectionCipherText))
        {
            aggregate.SetDatabaseConnection(snapshot.DatabaseConnectionCipherText);
        }

        if (!string.IsNullOrWhiteSpace(snapshot.RedisConnectionCipherText))
        {
            aggregate.SetRedisConnection(snapshot.RedisConnectionCipherText);
        }

        if (!string.IsNullOrWhiteSpace(snapshot.KeycloakAuthority) &&
            !string.IsNullOrWhiteSpace(snapshot.KeycloakRealm) &&
            !string.IsNullOrWhiteSpace(snapshot.KeycloakClientId) &&
            !string.IsNullOrWhiteSpace(snapshot.KeycloakClientSecretCipherText) &&
            Uri.TryCreate(snapshot.KeycloakAuthority, UriKind.Absolute, out var authorityUri))
        {
            aggregate.SetKeycloakConfiguration(
                authorityUri,
                snapshot.KeycloakRealm,
                snapshot.KeycloakClientId,
                snapshot.KeycloakClientSecretCipherText);
        }

        return aggregate.HasAnyValueConfigured ? aggregate : null;
    }

    private sealed record RuntimeConfigurationSnapshot(
        string? DatabaseConnectionCipherText,
        string? RedisConnectionCipherText,
        string? KeycloakAuthority,
        string? KeycloakRealm,
        string? KeycloakClientId,
        string? KeycloakClientSecretCipherText,
        DateTimeOffset UpdatedAtUtc);
}
