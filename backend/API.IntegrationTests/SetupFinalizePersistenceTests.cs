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

public sealed class SetupFinalizePersistenceTests
{
    [Fact]
    public async Task Finalize_Should_Persist_Verified_Database_Config_In_Status_After_Restart()
    {
        SharedInMemorySystemSettingRepository repository = new();
        DateTimeOffset seedTime = DateTimeOffset.UtcNow;
        repository.Seed(SystemSetting.Create(SetupSettingKeys.IsCompleted, bool.FalseString, seedTime));
        repository.Seed(SystemSetting.Create(SetupSettingKeys.CompletedAtUtc, string.Empty, seedTime));

        await using (var firstFactory = new SetupPersistenceApiWebApplicationFactory(repository))
        {
            using HttpClient firstClient = CreateAdminClient(firstFactory);

            HttpResponseMessage validateResponse = await firstClient.PostAsJsonAsync("/api/setup/validate-db", new
            {
                host = "db.internal",
                port = 5432,
                database = "qphising",
                username = "setup-admin",
                password = "TopSecret123!"
            });

            validateResponse.EnsureSuccessStatusCode();

            HttpResponseMessage finalizeResponse = await firstClient.PostAsync("/api/setup/finalize", content: null);
            finalizeResponse.EnsureSuccessStatusCode();
        }

        await using var restartedFactory = new SetupPersistenceApiWebApplicationFactory(repository);
        using HttpClient restartedClient = CreateAdminClient(restartedFactory);

        HttpResponseMessage statusResponse = await restartedClient.GetAsync("/api/setup/status");
        statusResponse.EnsureSuccessStatusCode();

        using JsonDocument payload = await JsonDocument.ParseAsync(await statusResponse.Content.ReadAsStreamAsync());
        JsonElement root = payload.RootElement;

        Assert.True(root.GetProperty("isCompleted").GetBoolean());
        Assert.True(root.GetProperty("hasPersistedDatabaseConfiguration").GetBoolean());

        string? persistedAt = root.GetProperty("persistedDatabaseConfigurationSavedAtUtc").GetString();
        Assert.False(string.IsNullOrWhiteSpace(persistedAt));
    }

    [Fact]
    public void MaskSecrets_Should_Redact_Password_And_Secret_Values()
    {
        const string rawMessage = "Database validation failed: Password=TopSecret123!;User Id=setup-admin; payload={\"password\":\"P@ssw0rd\",\"secret\":\"token-value\"}";

        string masked = QPhising.Infrastructure.Persistence.SetupSecretsMasker.MaskSecrets(rawMessage);

        Assert.DoesNotContain("TopSecret123!", masked, StringComparison.Ordinal);
        Assert.DoesNotContain("P@ssw0rd", masked, StringComparison.Ordinal);
        Assert.DoesNotContain("token-value", masked, StringComparison.Ordinal);
        Assert.Contains("Password=***", masked, StringComparison.Ordinal);
        Assert.Contains("\"password\":\"***\"", masked, StringComparison.Ordinal);
        Assert.Contains("\"secret\":\"***\"", masked, StringComparison.Ordinal);
    }

    private static HttpClient CreateAdminClient(ApiWebApplicationFactory factory)
    {
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Test");
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.RolesHeader, AuthorizationPolicies.Admin);
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.UserIdHeader, "admin-user");
        return client;
    }

    private sealed class SetupPersistenceApiWebApplicationFactory(SharedInMemorySystemSettingRepository repository) : ApiWebApplicationFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ISystemSettingRepository>();
                services.RemoveAll<IUnitOfWork>();
                services.RemoveAll<IDatabaseSetupValidator>();

                services.AddSingleton<ISystemSettingRepository>(repository);
                services.AddSingleton<IUnitOfWork, NoOpUnitOfWork>();
                services.AddSingleton<IDatabaseSetupValidator, AlwaysValidDatabaseSetupValidator>();
            });
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

    private sealed class AlwaysValidDatabaseSetupValidator : IDatabaseSetupValidator
    {
        public Task<DatabaseValidationResult> ValidateAsync(DatabaseConnectionInput input, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new DatabaseValidationResult(
                true,
                "Database connection succeeded and migrations are up to date.",
                null,
                0,
                "20260401010101_Initial",
                "20260401010101_Initial"));
        }
    }
}
