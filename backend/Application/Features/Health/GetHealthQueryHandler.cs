using MediatR;
using QPhising.Application.Common;

namespace QPhising.Application.Features.Health;

public sealed class GetHealthQueryHandler : IRequestHandler<GetHealthQuery, Result<HealthStatusDto>>
{
    public Task<Result<HealthStatusDto>> Handle(GetHealthQuery request, CancellationToken cancellationToken)
    {
        var payload = new HealthStatusDto("api", DateTimeOffset.UtcNow, "healthy");
        return Task.FromResult(Result<HealthStatusDto>.Success(payload));
    }
}
