using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QPhising.Infrastructure.Persistence;

public sealed class QPhisingDesignTimeDbContextFactory : IDesignTimeDbContextFactory<QPhisingDbContext>
{
    public QPhisingDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<QPhisingDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("Database__ConnectionString")
            ?? Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=qphising;Username=postgres;Password=postgres";

        builder.UseNpgsql(connectionString, npgsqlOptions =>
            npgsqlOptions.MigrationsAssembly(typeof(QPhisingDbContext).Assembly.FullName));

        return new QPhisingDbContext(builder.Options);
    }
}
