namespace QPhising.Application.Contracts.Responses.Identity;

public sealed record AccessTokenValidationResult(
    bool IsValid,
    bool IsExpired,
    string? Subject,
    IReadOnlyCollection<string> Roles,
    string? FailureReason)
{
    public static AccessTokenValidationResult Invalid(string failureReason, bool isExpired = false) =>
        new(
            IsValid: false,
            IsExpired: isExpired,
            Subject: null,
            Roles: Array.Empty<string>(),
            FailureReason: failureReason);
}
