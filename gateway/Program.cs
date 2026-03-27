using Gateway.Configuration;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Host.UseSerilog((context, _, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration).WriteTo.Console();
});

builder.Services
    .AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection(DatabaseOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "Database:ConnectionString is required.")
    .ValidateOnStart();

builder.Services
    .AddOptions<RedisOptions>()
    .Bind(builder.Configuration.GetSection(RedisOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "Redis:ConnectionString is required.")
    .ValidateOnStart();

builder.Services
    .AddOptions<KeycloakOptions>()
    .Bind(builder.Configuration.GetSection(KeycloakOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(options => Uri.IsWellFormedUriString(options.Authority, UriKind.Absolute), "Keycloak:Authority must be a valid absolute URL.")
    .ValidateOnStart();

builder.Services
    .AddOptions<SmtpOptions>()
    .Bind(builder.Configuration.GetSection(SmtpOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<BaseUrlOptions>()
    .Bind(builder.Configuration.GetSection(BaseUrlOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(options =>
        Uri.IsWellFormedUriString(options.Gateway, UriKind.Absolute) &&
        Uri.IsWellFormedUriString(options.Api, UriKind.Absolute) &&
        Uri.IsWellFormedUriString(options.Frontend, UriKind.Absolute),
        "BaseUrls:Gateway, BaseUrls:Api, and BaseUrls:Frontend must be valid absolute URLs.")
    .ValidateOnStart();

builder.Services.AddHealthChecks();
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseSerilogRequestLogging();
app.MapHealthChecks("/health", new HealthCheckOptions());

await app.UseOcelot();
await app.RunAsync();
