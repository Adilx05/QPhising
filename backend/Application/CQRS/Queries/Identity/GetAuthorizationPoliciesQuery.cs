using MediatR;
using QPhising.Application.Contracts.Responses.Identity;

namespace QPhising.Application.CQRS.Queries.Identity;

public sealed record GetAuthorizationPoliciesQuery : IRequest<AuthorizationPolicyCatalogResult>;
