using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions.Setup;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Configuration;

namespace QPhising.Application.Features.Setup.ValidateSso;

public sealed class ValidateSsoCommandHandler(
    ISsoSetupValidator ssoSetupValidator,
    ISystemSettingRepository systemSettingRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ValidateSsoCommand, Result<ValidateSsoResponse>>
{
    public async Task<Result<ValidateSsoResponse>> Handle(ValidateSsoCommand request, CancellationToken cancellationToken)
    {
        SsoValidationInput input = new(
            request.Authority,
            request.Realm,
            request.ClientId,
            request.ClientSecret,
            request.Audience);

        SsoValidationResult validationResult = await ssoSetupValidator.ValidateAsync(input, cancellationToken);

        DateTimeOffset now = DateTimeOffset.UtcNow;
        string readyValue = validationResult.IsValid ? bool.TrueString : bool.FalseString;
        SystemSetting? ssoReadySetting = await systemSettingRepository.GetByKeyAsync(SetupSettingKeys.SsoReady, cancellationToken);
        if (ssoReadySetting is null)
        {
            ssoReadySetting = SystemSetting.Create(SetupSettingKeys.SsoReady, readyValue, now);
            await systemSettingRepository.AddAsync(ssoReadySetting, cancellationToken);
        }
        else
        {
            ssoReadySetting.SetValue(readyValue, now);
            systemSettingRepository.Update(ssoReadySetting);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ValidateSsoResponse>.Success(new ValidateSsoResponse(
            validationResult.IsValid,
            validationResult.Message,
            validationResult.TechnicalReason,
            validationResult.FieldErrors));
    }
}
