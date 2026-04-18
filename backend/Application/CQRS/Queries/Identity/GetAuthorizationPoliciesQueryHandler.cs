using MediatR;
using QPhising.Application.Contracts.Responses.Identity;
using QPhising.Application.Security;

namespace QPhising.Application.CQRS.Queries.Identity;

public sealed class GetAuthorizationPoliciesQueryHandler : IRequestHandler<GetAuthorizationPoliciesQuery, AuthorizationPolicyCatalogResult>
{
    public Task<AuthorizationPolicyCatalogResult> Handle(GetAuthorizationPoliciesQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var policies = IdentityAuthorizationPolicies.CreateDefaultDefinitions();
        var result = new AuthorizationPolicyCatalogResult(policies);

        return Task.FromResult(result);
    }
}
