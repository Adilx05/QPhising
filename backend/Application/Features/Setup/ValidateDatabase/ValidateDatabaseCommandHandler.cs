using MediatR;
using Microsoft.Extensions.Logging;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions.Setup;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Configuration;
using System.Text.Json;

namespace QPhising.Application.Features.Setup.ValidateDatabase;

public sealed class ValidateDatabaseCommandHandler(
    IDatabaseSetupValidator databaseSetupValidator,
    ISetupAuditContext setupAuditContext,
    ISystemSettingRepository systemSettingRepository,
    IUnitOfWork unitOfWork,
    ILogger<ValidateDatabaseCommandHandler> logger)
    : IRequestHandler<ValidateDatabaseCommand, Result<ValidateDatabaseResponse>>
{
    public async Task<Result<ValidateDatabaseResponse>> Handle(ValidateDatabaseCommand request, CancellationToken cancellationToken)
    {
        DatabaseConnectionInput input = new(
            request.Host,
            request.Port,
            request.Database,
            request.Username,
            request.Password,
            request.ConnectionString);

        DatabaseValidationResult validationResult = await databaseSetupValidator.ValidateAsync(
            input,
            cancellationToken);

        if (validationResult.IsValid)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            string serializedConfiguration = JsonSerializer.Serialize(DatabaseConfigurationSnapshot.FromInput(input));
            string actor = setupAuditContext.GetActorIdentity();

            SystemSetting? validatedSetting = await systemSettingRepository.GetByKeyAsync(SetupSettingKeys.ValidatedDatabaseConfiguration, cancellationToken);
            if (validatedSetting is null)
            {
                validatedSetting = SystemSetting.Create(SetupSettingKeys.ValidatedDatabaseConfiguration, serializedConfiguration, now);
                await systemSettingRepository.AddAsync(validatedSetting, cancellationToken);
                logger.LogInformation(
                    "Setup audit: actor={Actor} changedAtUtc={ChangedAtUtc} changedField={ChangedField} action={Action}",
                    actor,
                    now,
                    SetupSettingKeys.ValidatedDatabaseConfiguration,
                    "create");
            }
            else
            {
                string previousValue = validatedSetting.Value;
                validatedSetting.SetValue(serializedConfiguration, now);
                systemSettingRepository.Update(validatedSetting);
                logger.LogInformation(
                    "Setup audit: actor={Actor} changedAtUtc={ChangedAtUtc} changedField={ChangedField} action={Action} previousValueHash={PreviousValueHash} newValueHash={NewValueHash}",
                    actor,
                    now,
                    SetupSettingKeys.ValidatedDatabaseConfiguration,
                    "update",
                    previousValue.GetHashCode(StringComparison.Ordinal),
                    serializedConfiguration.GetHashCode(StringComparison.Ordinal));
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<ValidateDatabaseResponse>.Success(new ValidateDatabaseResponse(
            validationResult.IsValid,
            validationResult.Message,
            validationResult.ErrorCategory,
            validationResult.PendingMigrationCount,
            validationResult.LastAppliedMigration,
            validationResult.LatestKnownMigration));
    }
}
