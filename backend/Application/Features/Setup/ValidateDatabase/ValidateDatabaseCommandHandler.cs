using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions.Setup;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Configuration;
using System.Text.Json;

namespace QPhising.Application.Features.Setup.ValidateDatabase;

public sealed class ValidateDatabaseCommandHandler(
    IDatabaseSetupValidator databaseSetupValidator,
    ISystemSettingRepository systemSettingRepository,
    IUnitOfWork unitOfWork)
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

            SystemSetting? validatedSetting = await systemSettingRepository.GetByKeyAsync(SetupSettingKeys.ValidatedDatabaseConfiguration, cancellationToken);
            if (validatedSetting is null)
            {
                validatedSetting = SystemSetting.Create(SetupSettingKeys.ValidatedDatabaseConfiguration, serializedConfiguration, now);
                await systemSettingRepository.AddAsync(validatedSetting, cancellationToken);
            }
            else
            {
                validatedSetting.SetValue(serializedConfiguration, now);
                systemSettingRepository.Update(validatedSetting);
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
