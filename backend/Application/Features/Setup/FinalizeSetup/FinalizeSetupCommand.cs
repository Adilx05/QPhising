using MediatR;
using QPhising.Application.Common;

namespace QPhising.Application.Features.Setup.FinalizeSetup;

public sealed record FinalizeSetupCommand() : IRequest<Result<FinalizeSetupResponse>>;

public sealed record FinalizeSetupResponse(bool IsCompleted, DateTimeOffset CompletedAtUtc);
