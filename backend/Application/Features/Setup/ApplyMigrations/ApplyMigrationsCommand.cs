using MediatR;
using QPhising.Application.Common;

namespace QPhising.Application.Features.Setup.ApplyMigrations;

public sealed record ApplyMigrationsCommand(
    string? Host,
    int? Port,
    string? Database,
    string? Username,
    string? Password,
    string? ConnectionString) : IRequest<Result<ApplyMigrationsResponse>>;

public sealed record ApplyMigrationsResponse(
    bool IsSuccess,
    string Message,
    string? ErrorCategory,
    int AppliedMigrationCount,
    int PendingMigrationCount,
    string? LastAppliedMigration,
    string? LatestKnownMigration);
