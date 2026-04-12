namespace QPhising.Application.Common.Abstractions.Setup;

public sealed record DatabaseMigrationApplyResult(
    bool IsSuccess,
    string Message,
    string? ErrorCategory,
    int AppliedMigrationCount,
    int PendingMigrationCount,
    string? LastAppliedMigration,
    string? LatestKnownMigration);

public interface IDatabaseMigrationSetupService
{
    Task<DatabaseMigrationApplyResult> ApplyMigrationsAsync(DatabaseConnectionInput input, CancellationToken cancellationToken = default);
}
