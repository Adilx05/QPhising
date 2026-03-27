using Microsoft.Extensions.DependencyInjection;
using QPhising.Worker.TaskExecution.Handlers;

namespace QPhising.Worker.TaskExecution;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTaskExecution(this IServiceCollection services)
    {
        services.AddScoped<IQueuedTaskDispatcher, QueuedTaskDispatcher>();
        services.AddScoped<IQueuedTaskHandlerRegistry, QueuedTaskHandlerRegistry>();

        services.AddScoped<IQueuedTaskHandler, TrackingLinkGenerationTaskHandler>();
        services.AddScoped<IQueuedTaskHandler, TrackingClickProcessingTaskHandler>();
        services.AddScoped<IQueuedTaskHandler, CampaignActivationTaskHandler>();
        services.AddScoped<IQueuedTaskHandler, ExportGenerationTaskHandler>();

        return services;
    }
}
