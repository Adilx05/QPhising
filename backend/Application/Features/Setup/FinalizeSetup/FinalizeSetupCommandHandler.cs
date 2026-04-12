using MediatR;
using Microsoft.Extensions.Logging;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions.Setup;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Configuration;
using System.Globalization;

namespace QPhising.Application.Features.Setup.FinalizeSetup;

public sealed class FinalizeSetupCommandHandler(
    ISetupAuditContext setupAuditContext,
    ISystemSettingRepository systemSettingRepository,
    IUnitOfWork unitOfWork,
    ILogger<FinalizeSetupCommandHandler> logger)
    : IRequestHandler<FinalizeSetupCommand, Result<FinalizeSetupResponse>>
{
    public async Task<Result<FinalizeSetupResponse>> Handle(FinalizeSetupCommand request, CancellationToken cancellationToken)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        string actor = setupAuditContext.GetActorIdentity();
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
            WriteAuditLog(actor, now, SetupSettingKeys.PersistedDatabaseConfiguration, "create");
        }
        else
        {
            persistedDatabaseConfigurationSetting.SetValue(validatedDatabaseConfigurationSetting.Value, now);
            systemSettingRepository.Update(persistedDatabaseConfigurationSetting);
            WriteAuditLog(actor, now, SetupSettingKeys.PersistedDatabaseConfiguration, "update");
        }

        SystemSetting? persistedDatabaseSavedAtSetting = await systemSettingRepository.GetByKeyAsync(SetupSettingKeys.PersistedDatabaseConfigurationSavedAtUtc, cancellationToken);
        if (persistedDatabaseSavedAtSetting is null)
        {
            persistedDatabaseSavedAtSetting = SystemSetting.Create(SetupSettingKeys.PersistedDatabaseConfigurationSavedAtUtc, now.ToString("O", CultureInfo.InvariantCulture), now);
            await systemSettingRepository.AddAsync(persistedDatabaseSavedAtSetting, cancellationToken);
            WriteAuditLog(actor, now, SetupSettingKeys.PersistedDatabaseConfigurationSavedAtUtc, "create");
        }
        else
        {
            persistedDatabaseSavedAtSetting.SetValue(now.ToString("O", CultureInfo.InvariantCulture), now);
            systemSettingRepository.Update(persistedDatabaseSavedAtSetting);
            WriteAuditLog(actor, now, SetupSettingKeys.PersistedDatabaseConfigurationSavedAtUtc, "update");
        }

        SystemSetting? completedSetting = await systemSettingRepository.GetByKeyAsync(SetupSettingKeys.IsCompleted, cancellationToken);
        if (completedSetting is null)
        {
            completedSetting = SystemSetting.Create(SetupSettingKeys.IsCompleted, bool.TrueString, now);
            await systemSettingRepository.AddAsync(completedSetting, cancellationToken);
            WriteAuditLog(actor, now, SetupSettingKeys.IsCompleted, "create");
        }
        else
        {
            completedSetting.SetValue(bool.TrueString, now);
            systemSettingRepository.Update(completedSetting);
            WriteAuditLog(actor, now, SetupSettingKeys.IsCompleted, "update");
        }

        SystemSetting? completedAtSetting = await systemSettingRepository.GetByKeyAsync(SetupSettingKeys.CompletedAtUtc, cancellationToken);
        if (completedAtSetting is null)
        {
            completedAtSetting = SystemSetting.Create(SetupSettingKeys.CompletedAtUtc, now.ToString("O", CultureInfo.InvariantCulture), now);
            await systemSettingRepository.AddAsync(completedAtSetting, cancellationToken);
            WriteAuditLog(actor, now, SetupSettingKeys.CompletedAtUtc, "create");
        }
        else
        {
            completedAtSetting.SetValue(now.ToString("O", CultureInfo.InvariantCulture), now);
            systemSettingRepository.Update(completedAtSetting);
            WriteAuditLog(actor, now, SetupSettingKeys.CompletedAtUtc, "update");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<FinalizeSetupResponse>.Success(new FinalizeSetupResponse(true, now));
    }

    private void WriteAuditLog(string actor, DateTimeOffset changedAtUtc, string changedField, string action)
    {
        logger.LogInformation(
            "Setup audit: actor={Actor} changedAtUtc={ChangedAtUtc} changedField={ChangedField} action={Action}",
            actor,
            changedAtUtc,
            changedField,
            action);
    }
}
