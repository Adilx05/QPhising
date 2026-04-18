namespace QPhising.Application.Contracts.Responses.Identity;

public sealed record AuthorizationPolicyDefinitionResult(
    string PolicyName,
    IReadOnlyCollection<string> RequiredRoles);
