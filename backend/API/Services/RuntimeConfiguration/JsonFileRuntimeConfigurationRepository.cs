using System.Linq;
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

            var aggregate = FromNode(runtimeConfigurationNode);
            if (aggregate is null)
            {
                return null;
            }

            if (aggregate.RedisConnectionCipherText is null)
            {
                ApplySetupRedisFallback(aggregate, rootNode);
            }

            return aggregate;
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

    private static RuntimeConfigurationAggregate? FromNode(JsonNode runtimeConfigurationNode)
    {
        var runtimeNodeObject = runtimeConfigurationNode as JsonObject;
        if (runtimeNodeObject is null)
        {
            return null;
        }

        var aggregate = new RuntimeConfigurationAggregate();

        var databaseCipherText = GetStringValue(runtimeNodeObject, "databaseConnectionCipherText", "DatabaseConnectionCipherText");
        if (!string.IsNullOrWhiteSpace(databaseCipherText))
        {
            aggregate.SetDatabaseConnection(databaseCipherText);
        }

        var redisCipherText = GetStringValue(
            runtimeNodeObject,
            "redisConnectionCipherText",
            "RedisConnectionCipherText",
            "redisConnectionCiphertext");
        if (!string.IsNullOrWhiteSpace(redisCipherText))
        {
            aggregate.SetRedisConnection(redisCipherText);
        }

        var keycloakAuthority = GetStringValue(runtimeNodeObject, "keycloakAuthority", "KeycloakAuthority");
        var keycloakRealm = GetStringValue(runtimeNodeObject, "keycloakRealm", "KeycloakRealm");
        var keycloakClientId = GetStringValue(runtimeNodeObject, "keycloakClientId", "KeycloakClientId");
        var keycloakClientSecretCipherText = GetStringValue(
            runtimeNodeObject,
            "keycloakClientSecretCipherText",
            "KeycloakClientSecretCipherText",
            "keycloakClientSecretCiphertext");

        if (!string.IsNullOrWhiteSpace(keycloakAuthority) &&
            !string.IsNullOrWhiteSpace(keycloakRealm) &&
            !string.IsNullOrWhiteSpace(keycloakClientId) &&
            !string.IsNullOrWhiteSpace(keycloakClientSecretCipherText) &&
            Uri.TryCreate(keycloakAuthority, UriKind.Absolute, out var authorityUri))
        {
            aggregate.SetKeycloakConfiguration(
                authorityUri,
                keycloakRealm,
                keycloakClientId,
                keycloakClientSecretCipherText);
        }

        return aggregate.HasAnyValueConfigured ? aggregate : null;
    }

    private static string? GetStringValue(JsonObject node, params string[] keys)
    {
        foreach (var key in keys)
        {
            var value = node[key]?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        foreach (var property in node)
        {
            if (property.Value is null)
            {
                continue;
            }

            if (keys.Any(candidate => string.Equals(candidate, property.Key, StringComparison.OrdinalIgnoreCase)))
            {
                var value = property.Value.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }

        return null;
    }

    private static void ApplySetupRedisFallback(RuntimeConfigurationAggregate aggregate, JsonObject? rootNode)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        if (rootNode is null)
        {
            return;
        }

        if (rootNode["SetupWizardState"] is not JsonObject setupNode)
        {
            return;
        }

        var setupRedisCipherText = GetStringValue(
            setupNode,
            "redisConnectionCipherText",
            "RedisConnectionCipherText",
            "redisConnectionCiphertext");

        if (!string.IsNullOrWhiteSpace(setupRedisCipherText))
        {
            aggregate.SetRedisConnection(setupRedisCipherText);
        }
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
