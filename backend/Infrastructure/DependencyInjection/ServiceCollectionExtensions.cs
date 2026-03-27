using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QPhising.Application.Common.Abstractions;
using QPhising.Application.Common.Abstractions.Exports;
using QPhising.Domain.Abstractions;
using QPhising.Infrastructure.Exports;
using QPhising.Infrastructure.Persistence;
using QPhising.Infrastructure.Persistence.Repositories;
using QPhising.Infrastructure.Security;
using StackExchange.Redis;

namespace QPhising.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "Database:ConnectionString is required.")
            .ValidateOnStart();

        services
            .AddOptions<RedisOptions>()
            .Bind(configuration.GetSection(RedisOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "Redis:ConnectionString is required.")
            .ValidateOnStart();

        services
            .AddOptions<TrackingTokenOptions>()
            .Bind(configuration.GetSection(TrackingTokenOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(options => !string.IsNullOrWhiteSpace(options.SigningKey), "TrackingTokens:SigningKey is required.")
            .ValidateOnStart();

        services
            .AddOptions<TrackingRetentionOptions>()
            .Bind(configuration.GetSection(TrackingRetentionOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<InfrastructureOptions>()
            .Configure<DatabaseOptions, RedisOptions>((options, database, redis) =>
            {
                options.DatabaseConnectionString = database.ConnectionString;
                options.RedisConnectionString = redis.ConnectionString;
            })
            .Validate(options => !string.IsNullOrWhiteSpace(options.DatabaseConnectionString), "Infrastructure database configuration is missing.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.RedisConnectionString), "Infrastructure Redis configuration is missing.")
            .ValidateOnStart();

        string connectionString = configuration.GetRequiredSection(DatabaseOptions.SectionName)[nameof(DatabaseOptions.ConnectionString)]
            ?? throw new InvalidOperationException("Database:ConnectionString is required.");
        string redisConnectionString = configuration.GetRequiredSection(RedisOptions.SectionName)[nameof(RedisOptions.ConnectionString)]
            ?? throw new InvalidOperationException("Redis:ConnectionString is required.");

        services.AddDbContext<QPhisingDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsAssembly(typeof(QPhisingDbContext).Assembly.FullName)));

        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<ITemplateRepository, TemplateRepository>();
        services.AddScoped<ITrackingClickRepository, TrackingClickRepository>();
        services.AddScoped<IQueuedTaskRepository, QueuedTaskRepository>();
        services.AddScoped<ITaskExecutionLogRepository, TaskExecutionLogRepository>();
        services.AddScoped<IAnalyticsReadRepository, AnalyticsReadRepository>();
        services.AddScoped<IAnalyticsDashboardCache, RedisAnalyticsDashboardCache>();
        services.AddScoped<IExcelExportService, ClosedXmlExcelExportService>();
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddScoped<ITrackingClickRealtimeStore, RedisTrackingClickRealtimeStore>();
        services.AddHostedService<TrackingRetentionBackgroundService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<ITrackingTokenService, HmacTrackingTokenService>();

        services.AddHealthChecks().AddCheck<InfrastructureOptionsHealthCheck>(
            "infrastructure-config",
            tags: ["ready"]);

        return services;
    }
}
