using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using QPhising.API.Configuration;
using QPhising.Application.DependencyInjection;
using QPhising.Infrastructure.DependencyInjection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
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

var keycloakSection = builder.Configuration.GetSection(KeycloakOptions.SectionName);
var keycloakOptions = keycloakSection.Get<KeycloakOptions>()
                      ?? throw new InvalidOperationException("Keycloak configuration is required.");

builder.Services
    .AddOptions<KeycloakOptions>()
    .Bind(keycloakSection)
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
        Uri.IsWellFormedUriString(options.Frontend, UriKind.Absolute),
        "BaseUrls:Gateway and BaseUrls:Frontend must be valid absolute URLs.")
    .ValidateOnStart();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakOptions.Authority;
        options.Audience = keycloakOptions.Audience;
        options.RequireHttpsMetadata = keycloakOptions.RequireHttpsMetadata;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions());

app.Run();
