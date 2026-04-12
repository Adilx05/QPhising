using MediatR;
using QPhising.Application.Common;

namespace QPhising.Application.Features.Setup.ValidateSso;

public sealed record ValidateSsoCommand(
    string Authority,
    string Realm,
    string ClientId,
    string ClientSecret,
    string Audience) : IRequest<Result<ValidateSsoResponse>>;

public sealed record ValidateSsoResponse(
    bool IsValid,
    string Message,
    string? TechnicalReason,
    IReadOnlyDictionary<string, string[]> FieldErrors);
