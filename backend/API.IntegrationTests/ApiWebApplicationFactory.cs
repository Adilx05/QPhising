using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QPhising.Application.Common.Abstractions;
using QPhising.Application.Features.Analytics.GetDashboardKpis;
using QPhising.Application.Features.Setup;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Configuration;
using QPhising.Infrastructure.Security;

namespace QPhising.API.IntegrationTests;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ =>
            {
            });

            services.AddAuthorizationBuilder()
                .SetFallbackPolicy(new AuthorizationPolicyBuilder(TestAuthHandler.SchemeName)
                    .RequireAuthenticatedUser()
                    .Build())
                .AddPolicy(AuthorizationPolicies.Admin, policy => policy.RequireRole(AuthorizationPolicies.Admin))
                .AddPolicy(AuthorizationPolicies.Operator, policy => policy.RequireRole(AuthorizationPolicies.Operator, AuthorizationPolicies.Admin))
                .AddPolicy(AuthorizationPolicies.Viewer, policy => policy.RequireRole(AuthorizationPolicies.Viewer, AuthorizationPolicies.Operator, AuthorizationPolicies.Admin));

            services.RemoveAll<IAnalyticsReadRepository>();
            services.RemoveAll<IAnalyticsDashboardCache>();
            services.RemoveAll<ISystemSettingRepository>();
            services.AddSingleton<IAnalyticsReadRepository, TestAnalyticsReadRepository>();
            services.AddSingleton<IAnalyticsDashboardCache, TestAnalyticsDashboardCache>();
            services.AddSingleton<ISystemSettingRepository>(_ =>
            {
                var now = DateTimeOffset.UtcNow;
                var repository = new InMemorySystemSettingRepository();
                repository.Seed(SystemSetting.Create(SetupSettingKeys.IsCompleted, bool.TrueString, now));
                repository.Seed(SystemSetting.Create(SetupSettingKeys.CompletedAtUtc, now.ToString("O"), now));
                return repository;
            });

            services.PostConfigure<TrackingTokenOptions>(options =>
            {
                options.SigningKey = "integration-test-signing-key-minimum-32chars";
                options.ExpirationMinutes = 30;
                options.Version = 1;
            });
        });
    }

    private sealed class InMemorySystemSettingRepository : ISystemSettingRepository
    {
        private readonly Dictionary<string, SystemSetting> _settings = new(StringComparer.Ordinal);

        public Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            _settings.TryGetValue(key, out var setting);
            return Task.FromResult(setting);
        }

        public Task AddAsync(SystemSetting systemSetting, CancellationToken cancellationToken = default)
        {
            _settings[systemSetting.Key] = systemSetting;
            return Task.CompletedTask;
        }

        public void Update(SystemSetting systemSetting)
        {
            _settings[systemSetting.Key] = systemSetting;
        }

        public void Seed(SystemSetting systemSetting)
        {
            _settings[systemSetting.Key] = systemSetting;
        }
    }

    private sealed class TestAnalyticsReadRepository : IAnalyticsReadRepository
    {
        public Task<AnalyticsDashboardReadModel> GetDashboardReadModelAsync(AnalyticsReadCriteria criteria, CancellationToken cancellationToken = default)
        {
            AnalyticsDashboardReadModel readModel = new(
                CampaignTotal: 1,
                CampaignDraft: 0,
                CampaignScheduled: 0,
                CampaignActive: 1,
                CampaignEnded: 0,
                CampaignArchived: 0,
                ClickTotal: 1,
                ClickUnique: 1,
                ConversionTotal: 0,
                TasksEnqueued: 1,
                TasksProcessed: 1,
                TasksSucceeded: 1,
                TasksFailed: 0,
                TasksRetried: 0,
                TasksDeadLettered: 0,
                AverageTaskDurationMilliseconds: 1m,
                Trend: [],
                CampaignStatusBreakdown: [],
                TemplateTypeBreakdown: []);

            return Task.FromResult(readModel);
        }
    }

    private sealed class TestAnalyticsDashboardCache : IAnalyticsDashboardCache
    {
        public Task<DashboardKpisResponse?> GetAsync(GetDashboardKpisQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<DashboardKpisResponse?>(null);
        }

        public Task SetAsync(GetDashboardKpisQuery query, DashboardKpisResponse response, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task InvalidateAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
