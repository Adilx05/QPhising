using FluentValidation;

namespace QPhising.Application.CQRS.Queries.Identity;

public sealed class GetAuthorizationPoliciesQueryValidator : AbstractValidator<GetAuthorizationPoliciesQuery>
{
    public GetAuthorizationPoliciesQueryValidator()
    {
    }
}
