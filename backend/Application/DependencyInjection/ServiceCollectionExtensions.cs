using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using QPhising.Application.Behaviors;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions;

namespace QPhising.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(typeof(ServiceCollectionExtensions).Assembly);
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
        services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped<ICampaignInteractionGuard, CampaignInteractionGuard>();
        services.AddSingleton<ITemplateHtmlSanitizer, TemplateHtmlSanitizer>();
        services.AddSingleton<ITemplateVariableSubstitutionEngine, TemplateVariableSubstitutionEngine>();
        return services;
    }
}
