using MediatR;
using QPhising.Application.Contracts.Responses.Identity;

namespace QPhising.Application.CQRS.Commands.Identity;

public sealed record ValidateAccessTokenCommand(string AccessToken) : IRequest<AccessTokenValidationResult>;
