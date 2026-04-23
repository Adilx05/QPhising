using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QPhising.Api.Infrastructure.Persistence;

namespace QPhising.Api.Tests.Infrastructure;

public sealed class TestApiFactory : WebApplicationFactory<Program>
{
    private readonly string _contentRootPath = Path.Combine(Path.GetTempPath(), $"qphising-api-tests-{Guid.NewGuid():N}");
    private readonly string _inMemoryDbName = $"qphising-tests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseContentRoot(_contentRootPath);
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Database:ApplyMigrationsOnStartup"] = "false",
                    ["Database:ContinueOnMigrationFailure"] = "true",
                    ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=qphising-tests"
                });
        });

        builder.ConfigureTestServices(services =>
        {
            // Remove any previously registered DbContext options/registrations for the app's real DB provider
            services.RemoveAll<DbContextOptions<QPhisingDbContext>>();
            services.RemoveAll<QPhisingDbContext>();

            // Register a scoped QPhisingDbContext instance constructed with in-memory options.
            // We avoid AddDbContext here because AddDbContext may register provider services
            // that conflict with the application's provider (e.g. Npgsql) when present.
            services.AddScoped<QPhisingDbContext>(sp =>
            {
                var options = new DbContextOptionsBuilder<QPhisingDbContext>()
                    .UseInMemoryDatabase(_inMemoryDbName)
                    .Options;

                return new QPhisingDbContext(options);
            });

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
        {
            return;
        }

        if (Directory.Exists(_contentRootPath))
        {
            Directory.Delete(_contentRootPath, recursive: true);
        }
    }
}
