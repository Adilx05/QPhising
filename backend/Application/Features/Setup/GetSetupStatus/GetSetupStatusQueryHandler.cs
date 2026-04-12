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

        SetupStatusResponse response = new(isCompleted, completedAtUtc);

        return Result<SetupStatusResponse>.Success(response);
    }
}
