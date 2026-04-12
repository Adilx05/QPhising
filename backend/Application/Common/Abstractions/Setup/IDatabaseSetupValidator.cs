namespace QPhising.Application.Common.Abstractions.Setup;

public sealed record DatabaseConnectionInput(
    string? Host,
    int? Port,
    string? Database,
    string? Username,
    string? Password,
    string? ConnectionString);

public sealed record DatabaseValidationResult(
    bool IsValid,
    string Message,
    string? ErrorCategory,
    int PendingMigrationCount,
    string? LastAppliedMigration,
    string? LatestKnownMigration);

public interface IDatabaseSetupValidator
{
    Task<DatabaseValidationResult> ValidateAsync(DatabaseConnectionInput input, CancellationToken cancellationToken = default);
}
