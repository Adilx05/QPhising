using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions.Setup;

namespace QPhising.Application.Features.Setup.ApplyMigrations;

public sealed class ApplyMigrationsCommandHandler(IDatabaseMigrationSetupService migrationSetupService)
    : IRequestHandler<ApplyMigrationsCommand, Result<ApplyMigrationsResponse>>
{
    public async Task<Result<ApplyMigrationsResponse>> Handle(ApplyMigrationsCommand request, CancellationToken cancellationToken)
    {
        DatabaseMigrationApplyResult migrationResult = await migrationSetupService.ApplyMigrationsAsync(
            new DatabaseConnectionInput(
                request.Host,
                request.Port,
                request.Database,
                request.Username,
                request.Password,
                request.ConnectionString),
            cancellationToken);

        return Result<ApplyMigrationsResponse>.Success(new ApplyMigrationsResponse(
            migrationResult.IsSuccess,
            migrationResult.Message,
            migrationResult.ErrorCategory,
            migrationResult.AppliedMigrationCount,
            migrationResult.PendingMigrationCount,
            migrationResult.LastAppliedMigration,
            migrationResult.LatestKnownMigration));
    }
}
