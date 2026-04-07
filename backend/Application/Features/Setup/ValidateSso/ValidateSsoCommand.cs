using MediatR;
using QPhising.Application.Common;

namespace QPhising.Application.Features.Setup.ValidateSso;

public sealed record ValidateSsoCommand() : IRequest<Result<ValidateSsoResponse>>;

public sealed record ValidateSsoResponse(bool IsValid, string Message);
