using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using QPhising.Gateway.Middleware;
using QPhising.Gateway.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using QPhising.Gateway.Health;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
var gatewayRoutePolicySettingsProvider = new ConfigurationGatewayRoutePolicySettingsProvider(builder.Configuration);
var gatewayRoutePolicySettings = gatewayRoutePolicySettingsProvider.GetCurrent();

builder.Services.AddCors(options =>
{
    options.AddPolicy("GatewayCors", policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            return;
        }

        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = gatewayRoutePolicySettings.AuthenticationProviderKey;
        options.DefaultChallengeScheme = gatewayRoutePolicySettings.AuthenticationProviderKey;
    })
    .AddJwtBearer(gatewayRoutePolicySettings.AuthenticationProviderKey, options =>
    {
        options.Authority = builder.Configuration["Authentication:Jwt:Authority"];
        options.Audience = builder.Configuration["Authentication:Jwt:Audience"];

        if (bool.TryParse(builder.Configuration["Authentication:Jwt:RequireHttpsMetadata"], out var requireHttpsMetadata))
        {
            options.RequireHttpsMetadata = requireHttpsMetadata;
        }
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpClient("health-keycloak", client =>
{
    client.Timeout = TimeSpan.FromSeconds(3);
});
builder.Services.AddHttpClient("health-downstream-api", client =>
{
    client.Timeout = TimeSpan.FromSeconds(3);
});

builder.Services.AddHealthChecks()
    .AddCheck("process", () => HealthCheckResult.Healthy("Process is running."), tags: ["live"])
    .AddCheck<DownstreamApiReadinessHealthCheck>("downstream-api", tags: ["ready"])
    .AddCheck<KeycloakReadinessHealthCheck>("keycloak", tags: ["ready"]);
builder.Services.AddSingleton(gatewayRoutePolicySettingsProvider);
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseRouting();
app.UseCors("GatewayCors");
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseAuthentication();
app.UseMiddleware<ClaimsToHeadersForwardingMiddleware>();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("live"),
    ResponseWriter = OperationalHealthResponseWriter.WriteAsync
});
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
    ResponseWriter = OperationalHealthResponseWriter.WriteAsync
});

await app.UseOcelot();

app.Run();
