using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QPhising.Application.Common.Abstractions.Setup;
using QPhising.Application.Features.Setup;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Configuration;
using Xunit;

namespace QPhising.API.IntegrationTests;

public sealed class SetupSsoValidationTests
{
    [Fact]
    public async Task ValidateSso_Should_Return_FieldErrors_And_Set_Ready_When_Successful()
    {
        SharedInMemorySystemSettingRepository repository = new();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        repository.Seed(SystemSetting.Create(SetupSettingKeys.IsCompleted, bool.FalseString, now));

        QueueBasedSsoSetupValidator validator = new(
            new SsoValidationResult(
                false,
                "Client was not found in the selected realm.",
                "client_not_found",
                new Dictionary<string, string[]> { ["clientId"] = ["Client ID does not exist in this realm."] }),
            new SsoValidationResult(
                true,
                "SSO configuration is valid and token exchange succeeded.",
                "ready",
                new Dictionary<string, string[]>()));

        await using var factory = new SetupSsoValidationApiWebApplicationFactory(repository, validator);
        using HttpClient client = CreateAdminClient(factory);

        HttpResponseMessage firstResponse = await client.PostAsJsonAsync("/api/setup/validate-sso", CreateRequestPayload());
        firstResponse.EnsureSuccessStatusCode();

        using (JsonDocument firstPayload = await JsonDocument.ParseAsync(await firstResponse.Content.ReadAsStreamAsync()))
        {
            JsonElement firstRoot = firstPayload.RootElement;
            Assert.False(firstRoot.GetProperty("isValid").GetBoolean());
            Assert.Equal("client_not_found", firstRoot.GetProperty("technicalReason").GetString());
            Assert.Equal("Client ID does not exist in this realm.", firstRoot.GetProperty("fieldErrors").GetProperty("clientId")[0].GetString());
        }

        HttpResponseMessage secondResponse = await client.PostAsJsonAsync("/api/setup/validate-sso", CreateRequestPayload());
        secondResponse.EnsureSuccessStatusCode();

        using (JsonDocument secondPayload = await JsonDocument.ParseAsync(await secondResponse.Content.ReadAsStreamAsync()))
        {
            JsonElement secondRoot = secondPayload.RootElement;
            Assert.True(secondRoot.GetProperty("isValid").GetBoolean());
            Assert.Equal("ready", secondRoot.GetProperty("technicalReason").GetString());
        }

        HttpResponseMessage statusResponse = await client.GetAsync("/api/setup/status");
        statusResponse.EnsureSuccessStatusCode();

        using JsonDocument statusPayload = await JsonDocument.ParseAsync(await statusResponse.Content.ReadAsStreamAsync());
        Assert.True(statusPayload.RootElement.GetProperty("isSsoReady").GetBoolean());
    }

    private static object CreateRequestPayload()
    {
        return new
        {
            authority = "https://keycloak.internal",
            realm = "qphising",
            clientId = "admin-cli",
            clientSecret = "secret-value",
            audience = "qphising-api"
        };
    }

    private static HttpClient CreateAdminClient(ApiWebApplicationFactory factory)
    {
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Test");
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.RolesHeader, AuthorizationPolicies.Admin);
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.UserIdHeader, "admin-user");
        return client;
    }

    private sealed class SetupSsoValidationApiWebApplicationFactory(
        SharedInMemorySystemSettingRepository repository,
        QueueBasedSsoSetupValidator ssoSetupValidator) : ApiWebApplicationFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ISystemSettingRepository>();
                services.RemoveAll<IUnitOfWork>();
                services.RemoveAll<ISsoSetupValidator>();

                services.AddSingleton<ISystemSettingRepository>(repository);
                services.AddSingleton<IUnitOfWork, NoOpUnitOfWork>();
                services.AddSingleton<ISsoSetupValidator>(ssoSetupValidator);
            });
        }
    }

    private sealed class QueueBasedSsoSetupValidator(params SsoValidationResult[] results) : ISsoSetupValidator
    {
        private readonly Queue<SsoValidationResult> _results = new(results);

        public Task<SsoValidationResult> ValidateAsync(SsoValidationInput input, CancellationToken cancellationToken = default)
        {
            if (_results.TryDequeue(out SsoValidationResult? next))
            {
                return Task.FromResult(next);
            }

            return Task.FromResult(new SsoValidationResult(
                true,
                "SSO configuration is valid and token exchange succeeded.",
                "ready",
                new Dictionary<string, string[]>()));
        }
    }

    private sealed class SharedInMemorySystemSettingRepository : ISystemSettingRepository
    {
        private readonly Dictionary<string, SystemSetting> _settings = new(StringComparer.Ordinal);

        public Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            _settings.TryGetValue(key, out SystemSetting? setting);
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

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }
    }
}
