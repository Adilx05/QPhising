using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QPhising.Application.Common.Abstractions;
using QPhising.Application.Common.Abstractions.Exports;
using QPhising.Application.Common.Abstractions.Setup;
using QPhising.Domain.Abstractions;
using QPhising.Infrastructure.Exports;
using QPhising.Infrastructure.Persistence;
using QPhising.Infrastructure.Persistence.Repositories;
using QPhising.Infrastructure.Security;
using System.Net.Http.Headers;
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
            .AddOptions<KeycloakValidationOptions>()
            .Bind(configuration.GetSection(KeycloakValidationOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(options => Uri.IsWellFormedUriString(options.Authority, UriKind.Absolute), "Keycloak:Authority must be a valid absolute URL.")
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
            .AddOptions<ExportStorageOptions>()
            .Bind(configuration.GetSection(ExportStorageOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(options => !string.IsNullOrWhiteSpace(options.BasePath), "ExportStorage:BasePath is required.")
            .ValidateOnStart();

        services
            .AddOptions<ExportRetentionOptions>()
            .Bind(configuration.GetSection(ExportRetentionOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<InfrastructureOptions>()
            .Configure<IOptions<DatabaseOptions>, IOptions<RedisOptions>>((options, database, redis) =>
            {
                options.DatabaseConnectionString = database.Value.ConnectionString;
                options.RedisConnectionString = redis.Value.ConnectionString;
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
        services.AddScoped<IExportJobRepository, ExportJobRepository>();
        services.AddScoped<IAnalyticsReadRepository, AnalyticsReadRepository>();
        services.AddScoped<ISetupStateRepository, SetupStateRepository>();
        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
        services.AddScoped<IDatabaseSetupValidator, DatabaseSetupValidator>();
        services.AddHttpClient<ISsoSetupValidator, SsoSetupValidator>(client =>
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        services.AddScoped<IAnalyticsDashboardCache, RedisAnalyticsDashboardCache>();
        services.AddScoped<IExcelExportService, ClosedXmlExcelExportService>();
        services.AddScoped<IPdfExportService, QuestPdfExportService>();
        services.AddScoped<IExportFileStorage, LocalExportFileStorage>();
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddScoped<ITrackingClickRealtimeStore, RedisTrackingClickRealtimeStore>();
        services.AddHostedService<TrackingRetentionBackgroundService>();
        services.AddHostedService<ExportRetentionBackgroundService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<ITrackingTokenService, HmacTrackingTokenService>();

        services.AddHealthChecks().AddCheck<InfrastructureOptionsHealthCheck>(
            "infrastructure-config",
            tags: ["ready"]);

        return services;
    }
}
