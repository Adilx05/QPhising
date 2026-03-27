using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Host.UseSerilog((context, _, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration).WriteTo.Console();
});

builder.Services.AddHealthChecks();
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseSerilogRequestLogging();
app.MapHealthChecks("/health", new HealthCheckOptions());

await app.UseOcelot();
await app.RunAsync();
