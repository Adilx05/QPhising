using MediatR;
using QPhising.Application.Common;

namespace QPhising.Application.Features.Setup.ValidateDatabase;

public sealed record ValidateDatabaseCommand() : IRequest<Result<ValidateDatabaseResponse>>;

public sealed record ValidateDatabaseResponse(bool IsValid, string Message);
