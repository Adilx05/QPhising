using MediatR;
using QPhising.Application.Contracts.Abstractions.Authorization;
using QPhising.Application.Contracts.Responses.Identity;
using QPhising.Domain.Identity.Models;

namespace QPhising.Application.CQRS.Commands.Identity;

public sealed class ValidateAccessTokenCommandHandler : IRequestHandler<ValidateAccessTokenCommand, AccessTokenValidationResult>
{
    private readonly IAccessTokenValidator _accessTokenValidator;

    public ValidateAccessTokenCommandHandler(IAccessTokenValidator accessTokenValidator)
    {
        _accessTokenValidator = accessTokenValidator;
    }

    public async Task<AccessTokenValidationResult> Handle(ValidateAccessTokenCommand request, CancellationToken cancellationToken)
    {
        var validatedPrincipal = await _accessTokenValidator.ValidateAsync(request.AccessToken, cancellationToken);

        if (!validatedPrincipal.IsValid)
        {
            return AccessTokenValidationResult.Invalid(
                failureReason: validatedPrincipal.FailureReason ?? "Access token validation failed.",
                isExpired: validatedPrincipal.IsExpired);
        }

        var roleClaimMap = IdentityRoleClaimMap.CreateDefault();
        var claimSet = validatedPrincipal.Claims
            .Distinct()
            .ToHashSet();

        var resolvedRoles = roleClaimMap.Mappings
            .Where(mapping => mapping.AcceptedClaims.Any(claimSet.Contains))
            .Select(mapping => mapping.Role.ToString())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new AccessTokenValidationResult(
            IsValid: true,
            IsExpired: false,
            Subject: validatedPrincipal.Subject,
            Roles: resolvedRoles,
            FailureReason: null);
    }
}
