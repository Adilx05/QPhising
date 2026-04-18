using FluentValidation;
using MediatR;
using QPhising.Api.Middleware;
using QPhising.Api.Security;
using QPhising.Api.Services.ProxyValidation;
using QPhising.Api.Services.RuntimeConfiguration;
using QPhising.Api.Services.Setup;
using QPhising.Api.Swagger;
using QPhising.Application.Contracts.Abstractions.Authorization;
using QPhising.Application.Contracts.Abstractions.ProxyValidation;
using QPhising.Application.Contracts.Abstractions.RuntimeConfiguration;
using QPhising.Application.Contracts.Abstractions.Setup;
using QPhising.Application.CQRS.Behaviors;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.runtime.json", optional: true, reloadOnChange: true);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<GlobalProblemDetailsResponsesOperationFilter>();
    options.OperationFilter<SetupEndpointsExamplesOperationFilter>();
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
builder.Services.AddDataProtection();
builder.Services.AddHttpClient(nameof(SetupDependencyConnectionTester), client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddValidatorsFromAssembly(typeof(QPhising.Application.CQRS.Commands.ProxyValidation.AssertProxyContractSyncCommand).Assembly);
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(QPhising.Application.CQRS.Commands.ProxyValidation.AssertProxyContractSyncCommand).Assembly);
    config.AddOpenBehavior(typeof(RequestLoggingBehavior<,>));
    config.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddScoped<ICurrentUserContext, HttpContextCurrentUserContext>();
builder.Services.AddScoped<IProxyContractDriftValidator, FileTimestampProxyContractDriftValidator>();
builder.Services.AddScoped<ISetupDependencyConnectionTester, SetupDependencyConnectionTester>();
builder.Services.AddScoped<ISetupSecretCipher, DataProtectionSetupSecretCipher>();
builder.Services.AddScoped<IRuntimeConfigurationSecretCipher, DataProtectionRuntimeConfigurationSecretCipher>();
builder.Services.AddSingleton<ISetupConfigurationRepository, JsonFileSetupConfigurationRepository>();
builder.Services.AddSingleton<IRuntimeConfigurationRepository, JsonFileRuntimeConfigurationRepository>();

var app = builder.Build();

app.UseMiddleware<ProblemDetailsExceptionMiddleware>();

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

app.MapHealthChecks("/health/live");
app.MapControllers();

app.Run();
