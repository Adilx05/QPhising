using QPhising.Domain.Setup.Aggregates;
using QPhising.Domain.Setup.Enums;

namespace QPhising.Domain.Setup.Policies;

public static class SetupCompletionPolicy
{
    public static SetupAccessState ResolveAccessState(SetupAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        if (aggregate.IsSetupCompleted && aggregate.ReadinessState == SetupReadinessState.Ready)
        {
            return SetupAccessState.MainApplicationAccessible;
        }

        if (aggregate.IsDatabaseConfigured || aggregate.IsKeycloakConfigured)
        {
            return SetupAccessState.SetupInProgress;
        }

        return SetupAccessState.SetupRequired;
    }
}
