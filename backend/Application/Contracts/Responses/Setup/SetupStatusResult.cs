using QPhising.Domain.Setup.Aggregates;
using QPhising.Domain.Setup.Enums;

namespace QPhising.Application.Contracts.Responses.Setup;

public sealed record SetupStatusResult(
    bool IsDatabaseConfigured,
    bool IsKeycloakConfigured,
    bool IsRedisConfigured,
    SetupReadinessState ReadinessState)
{
    public static SetupStatusResult FromAggregate(SetupAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        return new SetupStatusResult(
            aggregate.IsDatabaseConfigured,
            aggregate.IsKeycloakConfigured,
            aggregate.IsRedisConfigured,
            aggregate.ReadinessState);
    }

    public static SetupStatusResult NotStarted() => new(
        IsDatabaseConfigured: false,
        IsKeycloakConfigured: false,
        IsRedisConfigured: false,
        ReadinessState: SetupReadinessState.NotStarted);
}
