using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace QPhising.Api.Infrastructure.Persistence;

public sealed class QPhisingDbContextFactory : IDesignTimeDbContextFactory<QPhisingDbContext>
{
    public QPhisingDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var basePath = Directory.GetCurrentDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddJsonFile("appsettings.runtime.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is required for migrations.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<QPhisingDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new QPhisingDbContext(optionsBuilder.Options);
    }
}
