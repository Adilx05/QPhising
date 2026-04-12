using MediatR;
using Microsoft.Extensions.Logging;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions.Setup;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Configuration;

namespace QPhising.Application.Features.Setup.ValidateSso;

public sealed class ValidateSsoCommandHandler(
    ISsoSetupValidator ssoSetupValidator,
    ISetupAuditContext setupAuditContext,
    ISystemSettingRepository systemSettingRepository,
    IUnitOfWork unitOfWork,
    ILogger<ValidateSsoCommandHandler> logger)
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
        string actor = setupAuditContext.GetActorIdentity();
        SystemSetting? ssoReadySetting = await systemSettingRepository.GetByKeyAsync(SetupSettingKeys.SsoReady, cancellationToken);
        if (ssoReadySetting is null)
        {
            ssoReadySetting = SystemSetting.Create(SetupSettingKeys.SsoReady, readyValue, now);
            await systemSettingRepository.AddAsync(ssoReadySetting, cancellationToken);
            logger.LogInformation(
                "Setup audit: actor={Actor} changedAtUtc={ChangedAtUtc} changedField={ChangedField} action={Action} newValue={NewValue}",
                actor,
                now,
                SetupSettingKeys.SsoReady,
                "create",
                readyValue);
        }
        else
        {
            string previousValue = ssoReadySetting.Value;
            ssoReadySetting.SetValue(readyValue, now);
            systemSettingRepository.Update(ssoReadySetting);
            logger.LogInformation(
                "Setup audit: actor={Actor} changedAtUtc={ChangedAtUtc} changedField={ChangedField} action={Action} previousValue={PreviousValue} newValue={NewValue}",
                actor,
                now,
                SetupSettingKeys.SsoReady,
                "update",
                previousValue,
                readyValue);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ValidateSsoResponse>.Success(new ValidateSsoResponse(
            validationResult.IsValid,
            validationResult.Message,
            validationResult.TechnicalReason,
            validationResult.FieldErrors));
    }
}
