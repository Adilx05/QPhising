using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Abstractions;
using System.Globalization;

namespace QPhising.Application.Features.Setup.GetSetupStatus;

public sealed class GetSetupStatusQueryHandler(ISystemSettingRepository systemSettingRepository)
    : IRequestHandler<GetSetupStatusQuery, Result<SetupStatusResponse>>
{
    public async Task<Result<SetupStatusResponse>> Handle(GetSetupStatusQuery request, CancellationToken cancellationToken)
    {
        var completedSetting = await systemSettingRepository.GetByKeyAsync(SetupSettingKeys.IsCompleted, cancellationToken);
        bool isCompleted = bool.TryParse(completedSetting?.Value, out bool completedValue) && completedValue;

        var completedAtSetting = await systemSettingRepository.GetByKeyAsync(SetupSettingKeys.CompletedAtUtc, cancellationToken);
        DateTimeOffset? completedAtUtc = DateTimeOffset.TryParse(
            completedAtSetting?.Value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out DateTimeOffset parsedCompletedAtUtc)
            ? parsedCompletedAtUtc
            : null;

        var persistedDatabaseConfigurationSetting = await systemSettingRepository.GetByKeyAsync(SetupSettingKeys.PersistedDatabaseConfiguration, cancellationToken);
        bool hasPersistedDatabaseConfiguration = !string.IsNullOrWhiteSpace(persistedDatabaseConfigurationSetting?.Value);

        var persistedDatabaseConfigurationSavedAtSetting = await systemSettingRepository.GetByKeyAsync(SetupSettingKeys.PersistedDatabaseConfigurationSavedAtUtc, cancellationToken);
        DateTimeOffset? persistedDatabaseConfigurationSavedAtUtc = DateTimeOffset.TryParse(
            persistedDatabaseConfigurationSavedAtSetting?.Value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out DateTimeOffset parsedPersistedDatabaseConfigurationSavedAtUtc)
            ? parsedPersistedDatabaseConfigurationSavedAtUtc
            : null;

        var ssoReadySetting = await systemSettingRepository.GetByKeyAsync(SetupSettingKeys.SsoReady, cancellationToken);
        bool isSsoReady = bool.TryParse(ssoReadySetting?.Value, out bool parsedSsoReady) && parsedSsoReady;

        SetupStatusResponse response = new(
            isCompleted,
            completedAtUtc,
            hasPersistedDatabaseConfiguration,
            persistedDatabaseConfigurationSavedAtUtc,
            isSsoReady);

        return Result<SetupStatusResponse>.Success(response);
    }
}
