namespace QPhising.Application.Common.Abstractions.Setup;

public sealed record SsoValidationInput(
    string Authority,
    string Realm,
    string ClientId,
    string ClientSecret,
    string Audience);

public sealed record SsoValidationResult(
    bool IsValid,
    string Message,
    string? TechnicalReason,
    IReadOnlyDictionary<string, string[]> FieldErrors);

public interface ISsoSetupValidator
{
    Task<SsoValidationResult> ValidateAsync(SsoValidationInput input, CancellationToken cancellationToken = default);
}
