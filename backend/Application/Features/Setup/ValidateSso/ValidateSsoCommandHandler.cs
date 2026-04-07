using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions.Setup;

namespace QPhising.Application.Features.Setup.ValidateSso;

public sealed class ValidateSsoCommandHandler(ISsoSetupValidator ssoSetupValidator)
    : IRequestHandler<ValidateSsoCommand, Result<ValidateSsoResponse>>
{
    public async Task<Result<ValidateSsoResponse>> Handle(ValidateSsoCommand request, CancellationToken cancellationToken)
    {
        (bool isValid, string message) = await ssoSetupValidator.ValidateAsync(cancellationToken);
        return Result<ValidateSsoResponse>.Success(new ValidateSsoResponse(isValid, message));
    }
}
