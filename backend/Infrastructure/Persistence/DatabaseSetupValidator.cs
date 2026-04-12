using Microsoft.EntityFrameworkCore;
using Npgsql;
using QPhising.Application.Common.Abstractions.Setup;

namespace QPhising.Infrastructure.Persistence;

public sealed class DatabaseSetupValidator : IDatabaseSetupValidator
{
    public async Task<DatabaseValidationResult> ValidateAsync(DatabaseConnectionInput input, CancellationToken cancellationToken = default)
    {
        string connectionString = DatabaseConnectionStringFactory.Build(input);

        try
        {
            await using NpgsqlConnection connection = new(connectionString);
            await connection.OpenAsync(cancellationToken);

            DbContextOptions<QPhisingDbContext> dbContextOptions = new DbContextOptionsBuilder<QPhisingDbContext>()
                .UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(QPhisingDbContext).Assembly.FullName))
                .Options;

            await using QPhisingDbContext dbContext = new(dbContextOptions);
            IReadOnlyList<string> appliedMigrations = (await dbContext.Database.GetAppliedMigrationsAsync(cancellationToken)).ToList();
            IReadOnlyList<string> pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
            IReadOnlyList<string> allMigrations = (await dbContext.Database.GetMigrationsAsync(cancellationToken)).ToList();

            string? lastAppliedMigration = appliedMigrations.LastOrDefault();
            string? latestKnownMigration = allMigrations.LastOrDefault();

            if (pendingMigrations.Count == 0)
            {
                return new DatabaseValidationResult(
                    true,
                    "Database connection succeeded and migrations are up to date.",
                    null,
                    0,
                    lastAppliedMigration,
                    latestKnownMigration);
            }

            return new DatabaseValidationResult(
                false,
                $"Database connection succeeded but {pendingMigrations.Count} migration(s) are pending.",
                null,
                pendingMigrations.Count,
                lastAppliedMigration,
                latestKnownMigration);
        }
        catch (Exception exception)
        {
            string errorCategory = DatabaseSetupErrorClassifier.Classify(exception);
            return new DatabaseValidationResult(
                false,
                $"Database validation failed: {exception.Message}",
                errorCategory,
                0,
                null,
                null);
        }
    }
}
