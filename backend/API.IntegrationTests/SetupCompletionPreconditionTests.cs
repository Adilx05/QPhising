using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QPhising.Application.Features.Setup;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Configuration;
using Xunit;

namespace QPhising.API.IntegrationTests;

public sealed class SetupCompletionPreconditionTests
{
    [Fact]
    public async Task NonSetup_Endpoint_Should_Return_Locked_When_Setup_Not_Completed()
    {
        await using var factory = new IncompleteSetupApiWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        HttpResponseMessage response = await client.GetAsync("/api/access/viewer");

        Assert.Equal(HttpStatusCode.Locked, response.StatusCode);
    }

    [Fact]
    public async Task Setup_Status_Endpoint_Should_Be_Accessible_When_Setup_Not_Completed()
    {
        await using var factory = new IncompleteSetupApiWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        HttpResponseMessage response = await client.GetAsync("/api/setup/status");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed class IncompleteSetupApiWebApplicationFactory : ApiWebApplicationFactory
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ISystemSettingRepository>();
                services.AddSingleton<ISystemSettingRepository>(_ =>
                {
                    var repository = new InMemorySystemSettingRepository();
                    var now = DateTimeOffset.UtcNow;
                    repository.Seed(SystemSetting.Create(SetupSettingKeys.IsCompleted, bool.FalseString, now));
                    repository.Seed(SystemSetting.Create(SetupSettingKeys.CompletedAtUtc, string.Empty, now));
                    return repository;
                });
            });
        }
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
}
