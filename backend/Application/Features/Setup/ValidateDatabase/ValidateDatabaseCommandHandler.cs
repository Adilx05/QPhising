using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions.Setup;

namespace QPhising.Application.Features.Setup.ValidateDatabase;

public sealed class ValidateDatabaseCommandHandler(IDatabaseSetupValidator databaseSetupValidator)
    : IRequestHandler<ValidateDatabaseCommand, Result<ValidateDatabaseResponse>>
{
    public async Task<Result<ValidateDatabaseResponse>> Handle(ValidateDatabaseCommand request, CancellationToken cancellationToken)
    {
        DatabaseValidationResult validationResult = await databaseSetupValidator.ValidateAsync(
            new DatabaseConnectionInput(
                request.Host,
                request.Port,
                request.Database,
                request.Username,
                request.Password,
                request.ConnectionString),
            cancellationToken);

        return Result<ValidateDatabaseResponse>.Success(new ValidateDatabaseResponse(
            validationResult.IsValid,
            validationResult.Message,
            validationResult.ErrorCategory,
            validationResult.PendingMigrationCount,
            validationResult.LastAppliedMigration,
            validationResult.LatestKnownMigration));
    }
}
