using AutoMapper;
using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Features.Setup;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Configuration;

namespace QPhising.Application.Features.Health;

public sealed class GetHealthQueryHandler(IMapper mapper, ISystemSettingRepository systemSettingRepository) : IRequestHandler<GetHealthQuery, Result<HealthStatusDto>>
{
    public async Task<Result<HealthStatusDto>> Handle(GetHealthQuery request, CancellationToken cancellationToken)
    {
        SystemSetting? completedSetting = await systemSettingRepository.GetByKeyAsync(SetupSettingKeys.IsCompleted, cancellationToken);
        bool isSetupCompleted = bool.TryParse(completedSetting?.Value, out bool parsedValue) && parsedValue;
        string setupStatus = isSetupCompleted ? "complete" : "pending";

        var healthStatus = new HealthStatus("api", DateTimeOffset.UtcNow, "healthy", setupStatus);
        var payload = mapper.Map<HealthStatusDto>(healthStatus);

        return Result<HealthStatusDto>.Success(payload);
    }
}
