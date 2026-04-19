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
            return;
        }

        if (method == "GET" && path == "/api/campaigns")
        {
            operation.Summary ??= "List campaigns with lifecycle and targeting details.";
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), new OpenApiArray
            {
                CreateCampaignResponseExample()
            });
            return;
        }

        if (method == "GET" && path == "/api/campaigns/{campaignid}")
        {
            operation.Summary ??= "Get a campaign by identifier.";
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), CreateCampaignResponseExample());
            return;
        }

        if (method == "POST" && path == "/api/campaigns")
        {
            operation.Summary ??= "Create a new campaign in Draft lifecycle state.";
            SetRequestExample(operation, CreateCampaignRequestExample());
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), CreateCampaignResponseExample());
            return;
        }

        if (method == "PUT" && path == "/api/campaigns/{campaignid}")
        {
            operation.Summary ??= "Update mutable campaign details while preserving lifecycle history.";
            SetRequestExample(operation, new OpenApiObject
            {
                ["name"] = new OpenApiString("Quarterly Security Awareness Campaign")
            });
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), CreateCampaignResponseExample());
            return;
        }


        if (method == "POST" && path == "/api/campaigns/{campaignid}/schedule")
        {
            operation.Summary ??= "Schedule campaign execution window.";
            SetRequestExample(operation, new OpenApiObject
            {
                ["startsAtUtc"] = new OpenApiString("2026-05-01T09:00:00Z"),
                ["endsAtUtc"] = new OpenApiString("2026-05-08T09:00:00Z")
            });
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), CreateScheduledCampaignResponseExample());
            return;
        }

        if (method == "POST" && path == "/api/campaigns/{campaignid}/start")
        {
            operation.Summary ??= "Transition a scheduled campaign to Active.";
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), CreateCampaignResponseExample(lifecycleState: 2));
            return;
        }

        if (method == "POST" && path == "/api/campaigns/{campaignid}/pause")
        {
            operation.Summary ??= "Pause an active campaign.";
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), CreateCampaignResponseExample(lifecycleState: 3));
            return;
        }

        if (method == "POST" && path == "/api/campaigns/{campaignid}/complete")
        {
            operation.Summary ??= "Complete a campaign lifecycle.";
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), CreateCampaignResponseExample(lifecycleState: 4));
            return;
        }

        if (method == "POST" && path == "/api/campaigns/{campaignid}/cancel")
        {
            operation.Summary ??= "Cancel a campaign lifecycle.";
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), CreateCampaignResponseExample(lifecycleState: 5));
            return;
        }

        if (method == "GET" && path == "/api/templates")
        {
            operation.Summary ??= "List templates with lifecycle and version metadata.";
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), new OpenApiArray
            {
                CreateTemplateResponseExample()
            });
            return;
        }

        if (method == "GET" && path == "/api/templates/{templateid}")
        {
            operation.Summary ??= "Get a template by identifier.";
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), CreateTemplateResponseExample());
            return;
        }

        if (method == "POST" && path == "/api/templates")
        {
            operation.Summary ??= "Create a new draft template.";
            SetRequestExample(operation, CreateTemplateRequestExample());
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), CreateTemplateResponseExample());
            return;
        }

        if (method == "PUT" && path == "/api/templates/{templateid}")
        {
            operation.Summary ??= "Update mutable fields for an existing draft template.";
            SetRequestExample(operation, CreateTemplateRequestExample());
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), CreateTemplateResponseExample(version: 2));
            return;
        }

        if (method == "POST" && path == "/api/templates/{templateid}/publish")
        {
            operation.Summary ??= "Publish a draft template for campaign usage.";
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), CreateTemplateResponseExample(lifecycleState: 1));
            return;
        }

        if (method == "POST" && path == "/api/templates/{templateid}/archive")
        {
            operation.Summary ??= "Archive a template to prevent future use.";
            SetResponseExample(operation, StatusCodes.Status200OK.ToString(), CreateTemplateResponseExample(lifecycleState: 2));
            return;
        }

        if (method == "DELETE" && path == "/api/templates/{templateid}")
        {
            operation.Summary ??= "Delete a template.";
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

    private static OpenApiObject CreateCampaignRequestExample()
    {
        return new OpenApiObject
        {
            ["name"] = new OpenApiString("Quarterly Security Awareness Campaign"),
            ["templateId"] = new OpenApiString("7d6f5ca5-8f98-4659-af5a-1ed7cb7f3f1c")
        };
    }

    private static OpenApiObject CreateScheduledCampaignResponseExample()
    {
        return CreateCampaignResponseExample(
            lifecycleState: 1,
            startsAtUtc: "2026-05-01T09:00:00Z",
            endsAtUtc: "2026-05-08T09:00:00Z");
    }

    private static OpenApiObject CreateCampaignResponseExample(
        int lifecycleState = 0,
        string? startsAtUtc = null,
        string? endsAtUtc = null)
    {
        return new OpenApiObject
        {
            ["id"] = new OpenApiString("c136af52-9309-4832-89a4-fcd7fd53dc58"),
            ["name"] = new OpenApiString("Quarterly Security Awareness Campaign"),
            ["templateId"] = new OpenApiString("7d6f5ca5-8f98-4659-af5a-1ed7cb7f3f1c"),
            ["lifecycleState"] = new OpenApiInteger(lifecycleState),
            ["startsAtUtc"] = startsAtUtc is null ? new OpenApiNull() : new OpenApiString(startsAtUtc),
            ["endsAtUtc"] = endsAtUtc is null ? new OpenApiNull() : new OpenApiString(endsAtUtc),
            ["createdAtUtc"] = new OpenApiString("2026-04-18T13:31:00Z"),
            ["updatedAtUtc"] = new OpenApiString("2026-04-18T13:40:00Z")
        };
    }

    private static OpenApiObject CreateTemplateRequestExample()
    {
        return new OpenApiObject
        {
            ["name"] = new OpenApiString("Credential Harvesting Alert - Finance"),
            ["htmlContent"] = new OpenApiString("<html><body><h1>Confirm Payroll Access</h1><form><input placeholder=\"Username\"/><input placeholder=\"Password\" type=\"password\"/></form></body></html>"),
            ["description"] = new OpenApiString("Finance-focused tracking page template."),
            ["tags"] = new OpenApiArray
            {
                new OpenApiString("finance"),
                new OpenApiString("credential-harvest")
            }
        };
    }

    private static OpenApiObject CreateTemplateResponseExample(
        int lifecycleState = 0,
        int version = 1)
    {
        return new OpenApiObject
        {
            ["id"] = new OpenApiString("f73de4d0-0c92-4455-b181-b4b9f17de886"),
            ["name"] = new OpenApiString("Credential Harvesting Alert - Finance"),
            ["htmlContent"] = new OpenApiString("<html><body><h1>Confirm Payroll Access</h1><form><input placeholder=\"Username\"/><input placeholder=\"Password\" type=\"password\"/></form></body></html>"),
            ["description"] = new OpenApiString("Finance-focused tracking page template."),
            ["tags"] = new OpenApiArray
            {
                new OpenApiString("finance"),
                new OpenApiString("credential-harvest")
            },
            ["lifecycleState"] = new OpenApiInteger(lifecycleState),
            ["version"] = new OpenApiInteger(version),
            ["createdAtUtc"] = new OpenApiString("2026-04-18T16:20:00Z"),
            ["updatedAtUtc"] = new OpenApiString("2026-04-18T16:45:00Z")
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
