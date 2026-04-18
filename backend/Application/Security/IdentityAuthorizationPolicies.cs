using QPhising.Application.Contracts.Responses.Identity;
using QPhising.Domain.Identity.Enums;

namespace QPhising.Application.Security;

public static class IdentityAuthorizationPolicies
{
    public const string AdminOnly = "Identity.AdminOnly";
    public const string OperatorOrAbove = "Identity.OperatorOrAbove";
    public const string ViewerOrAbove = "Identity.ViewerOrAbove";

    public static IReadOnlyCollection<AuthorizationPolicyDefinitionResult> CreateDefaultDefinitions() =>
    [
        new AuthorizationPolicyDefinitionResult(
            PolicyName: AdminOnly,
            RequiredRoles: [IdentityRole.Admin.ToString()]),

        new AuthorizationPolicyDefinitionResult(
            PolicyName: OperatorOrAbove,
            RequiredRoles: [IdentityRole.Admin.ToString(), IdentityRole.Operator.ToString()]),

        new AuthorizationPolicyDefinitionResult(
            PolicyName: ViewerOrAbove,
            RequiredRoles: [IdentityRole.Admin.ToString(), IdentityRole.Operator.ToString(), IdentityRole.Viewer.ToString()])
    ];
}
