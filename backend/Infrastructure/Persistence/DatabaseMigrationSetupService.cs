using Microsoft.EntityFrameworkCore;
using QPhising.Application.Common.Abstractions.Setup;

namespace QPhising.Infrastructure.Persistence;

public sealed class DatabaseMigrationSetupService : IDatabaseMigrationSetupService
{
    public async Task<DatabaseMigrationApplyResult> ApplyMigrationsAsync(DatabaseConnectionInput input, CancellationToken cancellationToken = default)
    {
        string connectionString = DatabaseConnectionStringFactory.Build(input);

        try
        {
            DbContextOptions<QPhisingDbContext> dbContextOptions = new DbContextOptionsBuilder<QPhisingDbContext>()
                .UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(QPhisingDbContext).Assembly.FullName))
                .Options;

            await using QPhisingDbContext dbContext = new(dbContextOptions);

            IReadOnlyList<string> pendingBefore = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
            await dbContext.Database.MigrateAsync(cancellationToken);

            IReadOnlyList<string> pendingAfter = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
            IReadOnlyList<string> appliedMigrations = (await dbContext.Database.GetAppliedMigrationsAsync(cancellationToken)).ToList();
            IReadOnlyList<string> allMigrations = (await dbContext.Database.GetMigrationsAsync(cancellationToken)).ToList();

            int appliedCount = Math.Max(0, pendingBefore.Count - pendingAfter.Count);
            string message = appliedCount == 0
                ? "No pending migrations found. Database is already up to date."
                : $"Applied {appliedCount} migration(s) successfully.";

            return new DatabaseMigrationApplyResult(
                true,
                message,
                null,
                appliedCount,
                pendingAfter.Count,
                appliedMigrations.LastOrDefault(),
                allMigrations.LastOrDefault());
        }
        catch (Exception exception)
        {
            string errorCategory = DatabaseSetupErrorClassifier.Classify(exception);
            string maskedError = SetupSecretsMasker.MaskSecrets(exception.Message);
            return new DatabaseMigrationApplyResult(
                false,
                $"Migration failed: {maskedError}",
                errorCategory,
                0,
                0,
                null,
                null);
        }
    }
}
