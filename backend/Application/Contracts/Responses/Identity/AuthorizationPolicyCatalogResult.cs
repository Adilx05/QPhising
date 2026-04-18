namespace QPhising.Application.Contracts.Responses.Identity;

public sealed record AuthorizationPolicyCatalogResult(
    IReadOnlyCollection<AuthorizationPolicyDefinitionResult> Policies);
