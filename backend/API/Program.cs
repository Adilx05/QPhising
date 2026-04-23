using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using QPhising.Api.Middleware;
using QPhising.Api.Security;
using QPhising.Api.Infrastructure.Persistence;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text.Json;
using QPhising.Api.Services.Gateway;
using QPhising.Api.Services.Identity;
using QPhising.Api.Services.ProxyValidation;
using QPhising.Api.Services.Reporting;
using QPhising.Api.Swagger;
using QPhising.Application.Contracts.Abstractions.Authorization;
using QPhising.Application.Contracts.Abstractions.Campaign;
using QPhising.Application.Contracts.Abstractions.Gateway;
using QPhising.Application.Contracts.Abstractions.Persistence;
using QPhising.Application.Contracts.Abstractions.ProxyValidation;
using QPhising.Application.Contracts.Abstractions.Reporting;
using QPhising.Application.Contracts.Abstractions.Template;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Application.CQRS.Behaviors;
using QPhising.Application.Security;
using System.Threading.RateLimiting;
using QuestPDF.Infrastructure;
using System.IO;

// Ensure the content root (used by WebApplicationFactory in tests) exists before building the host.
var contentRoot = Environment.GetEnvironmentVariable("ASPNETCORE_CONTENTROOT") ?? Directory.GetCurrentDirectory();
if (!Directory.Exists(contentRoot))
{
    Directory.CreateDirectory(contentRoot);
}

var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args, ContentRootPath = contentRoot });

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();
QuestPDF.Settings.License = LicenseType.Community;
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.Configure<TrackingReportBrandingOptions>(builder.Configuration.GetSection("TrackingReportBranding"));

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "QPhising API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
    });

    options.OperationFilter<AuthorizationMetadataOperationFilter>();
    options.OperationFilter<GlobalProblemDetailsResponsesOperationFilter>();
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            return;
        }

        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});
builder.Services.AddHealthChecks();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = (context, cancellationToken) =>
    {
        context.HttpContext.Response.Headers.RetryAfter = "60";
        return ValueTask.CompletedTask;
    };

    options.AddFixedWindowLimiter(
        policyName: RateLimitPolicies.PublicTrackingLanding,
        configureOptions =>
        {
            configureOptions.PermitLimit = 120;
            configureOptions.Window = TimeSpan.FromMinutes(1);
            configureOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            configureOptions.QueueLimit = 0;
        });

    options.AddFixedWindowLimiter(
        policyName: RateLimitPolicies.PublicVisitIngestion,
        configureOptions =>
        {
            configureOptions.PermitLimit = 60;
            configureOptions.Window = TimeSpan.FromMinutes(1);
            configureOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            configureOptions.QueueLimit = 0;
        });
});
builder.Services.AddDataProtection();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration.GetSection("Authentication:Jwt").Get<JwtValidationOptions>() ?? new JwtValidationOptions();
        options.Authority = jwtOptions.Authority;
        options.Audience = jwtOptions.Audience;
        options.RequireHttpsMetadata = jwtOptions.RequireHttpsMetadata;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "preferred_username",
            RoleClaimType = "roles"
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtRoleClaimMapper");

                try
                {
                    var identity = context.Principal?.Identity as ClaimsIdentity;
                    if (identity is null)
                    {
                        return Task.CompletedTask;
                    }

                    var discoveredRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    var existingRoles = identity.FindAll(ClaimTypes.Role)
                        .Select(static claim => claim.Value)
                        .Where(static role => !string.IsNullOrWhiteSpace(role));
                    discoveredRoles.UnionWith(existingRoles);

                    static IEnumerable<string> ExtractRolesFromJsonArray(JsonElement rolesElement)
                    {
                        if (rolesElement.ValueKind != JsonValueKind.Array)
                        {
                            return Enumerable.Empty<string>();
                        }

                        return rolesElement.EnumerateArray()
                            .Where(static element => element.ValueKind == JsonValueKind.String)
                            .Select(static element => element.GetString())
                            .Where(static role => !string.IsNullOrWhiteSpace(role))
                            .Select(static role => role!.Trim());
                    }

                    var realmAccessClaim = identity.FindFirst("realm_access")?.Value;
                    if (!string.IsNullOrWhiteSpace(realmAccessClaim))
                    {
                        using var realmAccessDocument = JsonDocument.Parse(realmAccessClaim);
                        if (realmAccessDocument.RootElement.TryGetProperty("roles", out var realmRolesElement))
                        {
                            discoveredRoles.UnionWith(ExtractRolesFromJsonArray(realmRolesElement));
                        }
                    }

                    var resourceAccessClaim = identity.FindFirst("resource_access")?.Value;
                    if (!string.IsNullOrWhiteSpace(resourceAccessClaim))
                    {
                        using var resourceAccessDocument = JsonDocument.Parse(resourceAccessClaim);
                        if (resourceAccessDocument.RootElement.TryGetProperty("qphising", out var qphisingResourceElement) &&
                            qphisingResourceElement.TryGetProperty("roles", out var qphisingRolesElement))
                        {
                            discoveredRoles.UnionWith(ExtractRolesFromJsonArray(qphisingRolesElement));
                        }
                    }

                    foreach (var role in discoveredRoles)
                    {
                        if (!identity.HasClaim(ClaimTypes.Role, role))
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, role));
                        }

                        if (!identity.HasClaim("roles", role))
                        {
                            identity.AddClaim(new Claim("roles", role));
                        }
                    }
                }
                catch (Exception exception)
                {
                    logger.LogWarning(exception, "JWT role claim mapping failed.");
                }

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization(options =>
{
    foreach (var policy in IdentityAuthorizationPolicies.CreateDefaultDefinitions())
    {
        options.AddPolicy(policy.PolicyName, configurePolicy =>
        {
            configurePolicy.RequireAuthenticatedUser();
            configurePolicy.RequireRole(policy.RequiredRoles.ToArray());
        });
    }
});
builder.Services.AddOptions<JwtValidationOptions>()
    .Bind(builder.Configuration.GetSection("Authentication:Jwt"));

builder.Services.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(_ =>
{
    var jwtOptions = builder.Configuration.GetSection("Authentication:Jwt").Get<JwtValidationOptions>() ?? new JwtValidationOptions();
    var authority = jwtOptions.Authority.TrimEnd('/');

    if (string.IsNullOrWhiteSpace(authority))
    {
        authority = "https://invalid.local";
    }

    var metadataAddress = $"{authority}/.well-known/openid-configuration";

    return new ConfigurationManager<OpenIdConnectConfiguration>(
        metadataAddress,
        new OpenIdConnectConfigurationRetriever(),
        new HttpDocumentRetriever { RequireHttps = jwtOptions.RequireHttpsMetadata });
});
builder.Services.AddValidatorsFromAssembly(typeof(QPhising.Application.CQRS.Commands.ProxyValidation.AssertProxyContractSyncCommand).Assembly);
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(QPhising.Application.CQRS.Commands.ProxyValidation.AssertProxyContractSyncCommand).Assembly);
    config.AddOpenBehavior(typeof(RequestLoggingBehavior<,>));
    config.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
    config.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddScoped<ICurrentUserContext, HttpContextCurrentUserContext>();
builder.Services.Configure<VisitorIpHashOptions>(builder.Configuration.GetSection("Tracking:VisitorPrivacy"));
builder.Services.AddScoped<IVisitorIpHashService, VisitorIpHashService>();
builder.Services.AddScoped<IAccessTokenValidator, KeycloakAccessTokenValidator>();
builder.Services.AddScoped<IGatewayRoutePolicySettingsProvider, ConfigurationGatewayRoutePolicySettingsProvider>();
builder.Services.AddDbContext<QPhisingDbContext>((serviceProvider, options) =>
{
    var connectionString = serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Connection string 'DefaultConnection' is required.");
    }

    options.UseNpgsql(connectionString);
});
builder.Services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();
builder.Services.AddScoped<ICampaignRepository, EfCoreCampaignRepository>();
builder.Services.AddScoped<ITemplateRepository, EfCoreTemplateRepository>();
builder.Services.AddScoped<ITrackingPageRepository, EfCoreTrackingPageRepository>();
builder.Services.AddScoped<IVisitEventRepository, EfCoreVisitEventRepository>();
builder.Services.AddScoped<ITrackingReportExporter, TrackingReportExporter>();
builder.Services.AddScoped<IProxyContractDriftValidator, FileTimestampProxyContractDriftValidator>();


var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ProblemDetailsExceptionMiddleware>();

{
    using var migrationScope = app.Services.CreateScope();
    var logger = migrationScope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("DatabaseMigration");
    var dbContext = migrationScope.ServiceProvider.GetRequiredService<QPhisingDbContext>();

    try
    {
        logger.LogInformation(
            "Checking and applying pending EF Core migrations for context {DbContext} in environment {EnvironmentName}.",
            nameof(QPhisingDbContext),
            app.Environment.EnvironmentName);

        dbContext.Database.Migrate();

        logger.LogInformation("EF Core migration check completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogCritical(
            ex,
            "Failed to apply EF Core migrations for context {DbContext}.",
            nameof(QPhisingDbContext));

        var continueOnMigrationFailure = app.Configuration.GetValue("Database:ContinueOnMigrationFailure", false);
        if (!continueOnMigrationFailure)
        {
            throw;
        }

        logger.LogWarning("Continuing startup due to Database:ContinueOnMigrationFailure=true.");
    }
}

var swaggerEnabled = app.Environment.IsDevelopment() || app.Configuration.GetValue("FeatureFlags:SwaggerEnabled", false);
if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("FrontendCors");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<SecurityAuditMiddleware>();

app.MapHealthChecks("/health/live");
app.MapControllers();

app.Run();

public partial class Program;
