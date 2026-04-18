using MediatR;
using QPhising.Application.Contracts.Abstractions.Authorization;
using QPhising.Application.Exceptions;

namespace QPhising.Application.CQRS.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserContext _currentUserContext;

    public AuthorizationBehavior(ICurrentUserContext currentUserContext)
    {
        _currentUserContext = currentUserContext;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IAuthorizableRequest authorizableRequest || authorizableRequest.RequiredRoles.Count == 0)
        {
            return await next();
        }

        if (!_currentUserContext.IsAuthenticated)
        {
            throw new ApplicationAuthorizationException("The current user is not authenticated.");
        }

        var hasRequiredRole = authorizableRequest.RequiredRoles
            .Any(requiredRole => _currentUserContext.Roles.Contains(requiredRole, StringComparer.OrdinalIgnoreCase));

        if (!hasRequiredRole)
        {
            throw new ApplicationAuthorizationException("The current user is not authorized to perform this operation.");
        }

        return await next();
    }
}
