using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Configuration;
using System.Globalization;

namespace QPhising.Application.Features.Setup.FinalizeSetup;

public sealed class FinalizeSetupCommandHandler(
    ISystemSettingRepository systemSettingRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<FinalizeSetupCommand, Result<FinalizeSetupResponse>>
{
    public async Task<Result<FinalizeSetupResponse>> Handle(FinalizeSetupCommand request, CancellationToken cancellationToken)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        SystemSetting? completedSetting = await systemSettingRepository.GetByKeyAsync(SetupSettingKeys.IsCompleted, cancellationToken);
        if (completedSetting is null)
        {
            completedSetting = SystemSetting.Create(SetupSettingKeys.IsCompleted, bool.TrueString, now);
            await systemSettingRepository.AddAsync(completedSetting, cancellationToken);
        }
        else
        {
            completedSetting.SetValue(bool.TrueString, now);
            systemSettingRepository.Update(completedSetting);
        }

        SystemSetting? completedAtSetting = await systemSettingRepository.GetByKeyAsync(SetupSettingKeys.CompletedAtUtc, cancellationToken);
        if (completedAtSetting is null)
        {
            completedAtSetting = SystemSetting.Create(SetupSettingKeys.CompletedAtUtc, now.ToString("O", CultureInfo.InvariantCulture), now);
            await systemSettingRepository.AddAsync(completedAtSetting, cancellationToken);
        }
        else
        {
            completedAtSetting.SetValue(now.ToString("O", CultureInfo.InvariantCulture), now);
            systemSettingRepository.Update(completedAtSetting);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<FinalizeSetupResponse>.Success(new FinalizeSetupResponse(true, now));
    }
}
