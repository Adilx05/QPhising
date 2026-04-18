using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace QPhising.Api.Swagger;

public sealed class SetupEndpointsExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        var relativePath = context.ApiDescription.RelativePath;
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return;
        }

        var path = $"/{relativePath.TrimStart('/')}".ToLowerInvariant();
        var method = context.ApiDescription.HttpMethod?.ToUpperInvariant();

        if (method == "GET" && path == "/api/setup/status")
        {
            operation.Summary ??= "Get current setup readiness status.";
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), CreateStatusResponseExample());
            return;
        }

        if (method == "GET" && path == "/api/setup/guard-decision")
        {
            operation.Summary ??= "Resolve setup-gating decision for setup wizard and main application access.";
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), CreateGuardDecisionResponseExample());
            return;
        }

        if (method == "POST" && path == "/api/setup/test-db")
        {
            operation.Summary ??= "Validate database connectivity using the supplied connection string.";
            SetRequestExample(operation, new OpenApiObject
            {
                ["connectionString"] = new OpenApiString("Host=localhost;Port=5432;Database=qphising;Username=qphising_user;Password=StrongPassword!123")
            });
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), CreateDependencyResponseExample("Database"));
            return;
        }

        if (method == "POST" && path == "/api/setup/test-redis")
        {
            operation.Summary ??= "Validate Redis connectivity using the supplied connection string.";
            SetRequestExample(operation, new OpenApiObject
            {
                ["connectionString"] = new OpenApiString("localhost:6379,password=StrongPassword!123,ssl=False,abortConnect=False")
            });
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), CreateDependencyResponseExample("Redis"));
            return;
        }

        if (method == "POST" && path == "/api/setup/test-keycloak")
        {
            operation.Summary ??= "Validate Keycloak authority, realm, and client credentials.";
            SetRequestExample(operation, new OpenApiObject
            {
                ["authority"] = new OpenApiString("https://keycloak.local/auth"),
                ["realm"] = new OpenApiString("qphising"),
                ["clientId"] = new OpenApiString("qphising-api"),
                ["clientSecret"] = new OpenApiString("super-secret-client-key")
            });
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), CreateDependencyResponseExample("Keycloak"));
            return;
        }

        if (method == "POST" && path == "/api/setup/save")
        {
            operation.Summary ??= "Persist setup configuration after successful dependency checks.";
            SetRequestExample(operation, new OpenApiObject
            {
                ["databaseConnectionString"] = new OpenApiString("Host=localhost;Port=5432;Database=qphising;Username=qphising_user;Password=StrongPassword!123"),
                ["redisConnectionString"] = new OpenApiString("localhost:6379,password=StrongPassword!123,ssl=False,abortConnect=False"),
                ["keycloakAuthority"] = new OpenApiString("https://keycloak.local/auth"),
                ["keycloakRealm"] = new OpenApiString("qphising"),
                ["keycloakClientId"] = new OpenApiString("qphising-api"),
                ["keycloakClientSecret"] = new OpenApiString("super-secret-client-key")
            });
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), new OpenApiObject
            {
                ["isDatabaseConfigured"] = new OpenApiBoolean(true),
                ["isKeycloakConfigured"] = new OpenApiBoolean(true),
                ["isRedisConfigured"] = new OpenApiBoolean(true),
                ["readinessState"] = new OpenApiInteger(2)
            });
            return;
        }

        if (method == "GET" && path == "/api/configuration")
        {
            operation.Summary ??= "Get current persisted runtime configuration readiness state.";
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), CreateRuntimeConfigurationResponseExample());
            return;
        }

        if (method == "POST" && path == "/api/configuration")
        {
            operation.Summary ??= "Persist full runtime configuration payload.";
            SetRequestExample(operation, CreateRuntimeConfigurationSaveRequestExample());
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), CreateRuntimeConfigurationResponseExample());
            return;
        }

        if (method == "PATCH" && path == "/api/configuration")
        {
            operation.Summary ??= "Update runtime configuration values selectively.";
            SetRequestExample(operation, CreateRuntimeConfigurationUpdateRequestExample());
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), CreateRuntimeConfigurationResponseExample());
        }
    }

    private static OpenApiObject CreateDependencyResponseExample(string dependency)
    {
        return new OpenApiObject
        {
            ["dependency"] = new OpenApiString(dependency),
            ["succeeded"] = new OpenApiBoolean(true),
            ["failureReason"] = new OpenApiNull()
        };
    }

    private static OpenApiObject CreateStatusResponseExample()
    {
        return new OpenApiObject
        {
            ["isDatabaseConfigured"] = new OpenApiBoolean(true),
            ["isKeycloakConfigured"] = new OpenApiBoolean(true),
            ["isRedisConfigured"] = new OpenApiBoolean(true),
            ["readinessState"] = new OpenApiInteger(2)
        };
    }

    private static OpenApiObject CreateGuardDecisionResponseExample()
    {
        return new OpenApiObject
        {
            ["accessState"] = new OpenApiInteger(1),
            ["isSetupCompleted"] = new OpenApiBoolean(false),
            ["allowSetupWizard"] = new OpenApiBoolean(true),
            ["allowMainApplication"] = new OpenApiBoolean(false),
            ["recommendedRedirectPath"] = new OpenApiString("/setup")
        };
    }

    private static OpenApiObject CreateRuntimeConfigurationSaveRequestExample()
    {
        return new OpenApiObject
        {
            ["databaseConnectionString"] = new OpenApiString("Host=localhost;Port=5432;Database=qphising;Username=qphising_user;Password=StrongPassword!123"),
            ["redisConnectionString"] = new OpenApiString("localhost:6379,password=StrongPassword!123,ssl=False,abortConnect=False"),
            ["keycloakAuthority"] = new OpenApiString("https://keycloak.local/auth"),
            ["keycloakRealm"] = new OpenApiString("qphising"),
            ["keycloakClientId"] = new OpenApiString("qphising-api"),
            ["keycloakClientSecret"] = new OpenApiString("super-secret-client-key")
        };
    }

    private static OpenApiObject CreateRuntimeConfigurationUpdateRequestExample()
    {
        return new OpenApiObject
        {
            ["redisConnectionString"] = new OpenApiString("localhost:6380,password=StrongPassword!123,ssl=False,abortConnect=False"),
            ["keycloakClientSecret"] = new OpenApiString("rotated-super-secret-client-key")
        };
    }

    private static OpenApiObject CreateRuntimeConfigurationResponseExample()
    {
        return new OpenApiObject
        {
            ["isDatabaseConfigured"] = new OpenApiBoolean(true),
            ["isRedisConfigured"] = new OpenApiBoolean(true),
            ["isKeycloakConfigured"] = new OpenApiBoolean(true),
            ["isReadyForProtectedRuntime"] = new OpenApiBoolean(true),
            ["updatedAtUtc"] = new OpenApiString("2026-04-18T06:49:16Z")
        };
    }

    private static void SetRequestExample(OpenApiOperation operation, IOpenApiAny example)
    {
        if (operation.RequestBody?.Content is null)
        {
            return;
        }

        if (operation.RequestBody.Content.TryGetValue("application/json", out var mediaType))
        {
            mediaType.Example = example;
        }
    }

    private static void SetResponseExample(OpenApiOperation operation, string statusCode, IOpenApiAny example)
    {
        if (!operation.Responses.TryGetValue(statusCode, out var response))
        {
            return;
        }

        if (response.Content.TryGetValue("application/json", out var mediaType))
        {
            mediaType.Example = example;
        }
    }
}
