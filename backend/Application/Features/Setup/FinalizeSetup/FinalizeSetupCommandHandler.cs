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
        SystemSetting? validatedDatabaseConfigurationSetting = await systemSettingRepository.GetByKeyAsync(SetupSettingKeys.ValidatedDatabaseConfiguration, cancellationToken);
        if (validatedDatabaseConfigurationSetting is null || string.IsNullOrWhiteSpace(validatedDatabaseConfigurationSetting.Value))
        {
            return Result<FinalizeSetupResponse>.Failure("Finalize setup requires a validated database configuration.");
        }

        SystemSetting? persistedDatabaseConfigurationSetting = await systemSettingRepository.GetByKeyAsync(SetupSettingKeys.PersistedDatabaseConfiguration, cancellationToken);
        if (persistedDatabaseConfigurationSetting is null)
        {
            persistedDatabaseConfigurationSetting = SystemSetting.Create(SetupSettingKeys.PersistedDatabaseConfiguration, validatedDatabaseConfigurationSetting.Value, now);
            await systemSettingRepository.AddAsync(persistedDatabaseConfigurationSetting, cancellationToken);
        }
        else
        {
            persistedDatabaseConfigurationSetting.SetValue(validatedDatabaseConfigurationSetting.Value, now);
            systemSettingRepository.Update(persistedDatabaseConfigurationSetting);
        }

        SystemSetting? persistedDatabaseSavedAtSetting = await systemSettingRepository.GetByKeyAsync(SetupSettingKeys.PersistedDatabaseConfigurationSavedAtUtc, cancellationToken);
        if (persistedDatabaseSavedAtSetting is null)
        {
            persistedDatabaseSavedAtSetting = SystemSetting.Create(SetupSettingKeys.PersistedDatabaseConfigurationSavedAtUtc, now.ToString("O", CultureInfo.InvariantCulture), now);
            await systemSettingRepository.AddAsync(persistedDatabaseSavedAtSetting, cancellationToken);
        }
        else
        {
            persistedDatabaseSavedAtSetting.SetValue(now.ToString("O", CultureInfo.InvariantCulture), now);
            systemSettingRepository.Update(persistedDatabaseSavedAtSetting);
        }

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
