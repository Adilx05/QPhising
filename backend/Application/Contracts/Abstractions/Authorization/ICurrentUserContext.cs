namespace QPhising.Application.Contracts.Abstractions.Authorization;

public interface ICurrentUserContext
{
    string? UserId { get; }

    bool IsAuthenticated { get; }

    IReadOnlyCollection<string> Roles { get; }
}
