using MediatR;
using QPhising.Api.Services.ProxyValidation;
using QPhising.Application.Contracts.Abstractions.ProxyValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.runtime.json", optional: true, reloadOnChange: true);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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
builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(typeof(QPhising.Application.CQRS.Commands.ProxyValidation.AssertProxyContractSyncCommand).Assembly));
builder.Services.AddScoped<IProxyContractDriftValidator, FileTimestampProxyContractDriftValidator>();

var app = builder.Build();

var swaggerEnabled = app.Environment.IsDevelopment() || app.Configuration.GetValue("FeatureFlags:SwaggerEnabled", false);
if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendCors");

app.MapHealthChecks("/health/live");
app.MapControllers();

app.Run();
