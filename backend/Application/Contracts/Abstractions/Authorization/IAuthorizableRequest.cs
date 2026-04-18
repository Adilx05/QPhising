namespace QPhising.Application.Contracts.Abstractions.Authorization;

public interface IAuthorizableRequest
{
    IReadOnlyCollection<string> RequiredRoles { get; }
}
