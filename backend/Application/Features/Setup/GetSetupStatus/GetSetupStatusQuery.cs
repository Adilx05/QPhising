using MediatR;
using QPhising.Application.Common;

namespace QPhising.Application.Features.Setup.GetSetupStatus;

public sealed record GetSetupStatusQuery() : IRequest<Result<SetupStatusResponse>>;

public sealed record SetupStatusResponse(
    bool IsCompleted,
    DateTimeOffset? CompletedAtUtc,
    bool HasPersistedDatabaseConfiguration,
    DateTimeOffset? PersistedDatabaseConfigurationSavedAtUtc,
    bool IsSsoReady);
