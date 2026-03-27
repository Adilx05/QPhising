using AutoMapper;
using MediatR;
using QPhising.Application.Common;

namespace QPhising.Application.Features.Health;

public sealed class GetHealthQueryHandler(IMapper mapper) : IRequestHandler<GetHealthQuery, Result<HealthStatusDto>>
{
    public Task<Result<HealthStatusDto>> Handle(GetHealthQuery request, CancellationToken cancellationToken)
    {
        var healthStatus = new HealthStatus("api", DateTimeOffset.UtcNow, "healthy");
        var payload = mapper.Map<HealthStatusDto>(healthStatus);

        return Task.FromResult(Result<HealthStatusDto>.Success(payload));
    }
}
