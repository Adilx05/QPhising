using MediatR;
using QPhising.Application.Common;

namespace QPhising.Application.Features.Health;

public sealed record GetHealthQuery : IRequest<Result<HealthStatusDto>>;

public sealed record HealthStatusDto(string Service, DateTimeOffset TimestampUtc, string Status, string SetupStatus);
