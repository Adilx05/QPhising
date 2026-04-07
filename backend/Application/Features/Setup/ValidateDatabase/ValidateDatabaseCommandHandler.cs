using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions.Setup;

namespace QPhising.Application.Features.Setup.ValidateDatabase;

public sealed class ValidateDatabaseCommandHandler(IDatabaseSetupValidator databaseSetupValidator)
    : IRequestHandler<ValidateDatabaseCommand, Result<ValidateDatabaseResponse>>
{
    public async Task<Result<ValidateDatabaseResponse>> Handle(ValidateDatabaseCommand request, CancellationToken cancellationToken)
    {
        (bool isValid, string message) = await databaseSetupValidator.ValidateAsync(cancellationToken);
        return Result<ValidateDatabaseResponse>.Success(new ValidateDatabaseResponse(isValid, message));
    }
}
