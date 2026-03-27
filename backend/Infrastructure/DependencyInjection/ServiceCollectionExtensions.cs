using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QPhising.Domain.Abstractions;
using QPhising.Infrastructure.Persistence;

namespace QPhising.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<InfrastructureOptions>()
            .Bind(configuration.GetSection(InfrastructureOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IUnitOfWork, UnitOfWork>();
        services.AddHealthChecks().AddCheck<InfrastructureOptionsHealthCheck>("infrastructure-config");

        return services;
    }
}
