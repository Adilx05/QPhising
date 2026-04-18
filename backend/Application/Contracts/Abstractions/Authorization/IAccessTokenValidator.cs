using QPhising.Domain.Identity.ValueObjects;

namespace QPhising.Application.Contracts.Abstractions.Authorization;

public interface IAccessTokenValidator
{
    Task<ValidatedAccessTokenPrincipal> ValidateAsync(string accessToken, CancellationToken cancellationToken);
}

public sealed record ValidatedAccessTokenPrincipal(
    bool IsValid,
    bool IsExpired,
    string? Subject,
    IReadOnlyCollection<IdentityClaim> Claims,
    string? FailureReason)
{
    public static ValidatedAccessTokenPrincipal Invalid(string failureReason, bool isExpired = false) =>
        new(
            IsValid: false,
            IsExpired: isExpired,
            Subject: null,
            Claims: Array.Empty<IdentityClaim>(),
            FailureReason: failureReason);
}
