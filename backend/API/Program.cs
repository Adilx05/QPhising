using MediatR;
using QPhising.Api.Services.ProxyValidation;
using QPhising.Application.Contracts.Abstractions.ProxyValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(typeof(QPhising.Application.CQRS.Commands.ProxyValidation.AssertProxyContractSyncCommand).Assembly));
builder.Services.AddScoped<IProxyContractDriftValidator, FileTimestampProxyContractDriftValidator>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
