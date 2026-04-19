using QPhising.Domain.Setup.Aggregates;
using QPhising.Domain.Setup.Enums;
using QPhising.Domain.Setup.Policies;

namespace QPhising.Application.Contracts.Responses.Setup;

public sealed record SetupGuardDecisionResult(
    SetupAccessState AccessState,
    bool IsSetupCompleted,
    bool AllowSetupWizard,
    bool AllowMainApplication,
    string RecommendedRedirectPath)
{
    private const string SetupWizardRoute = "/setup";
    private const string MainApplicationRoute = "/";

    public static SetupGuardDecisionResult FromAggregate(SetupAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        var accessState = SetupCompletionPolicy.ResolveAccessState(aggregate);

        return FromAccessState(accessState, aggregate.IsSetupCompleted);
    }

    public static SetupGuardDecisionResult SetupRequired() =>
        FromAccessState(SetupAccessState.SetupRequired, isSetupCompleted: false);

    public static SetupGuardDecisionResult MainApplicationAccessibleWithoutWizard() =>
        FromAccessState(SetupAccessState.MainApplicationAccessible, isSetupCompleted: true);

    private static SetupGuardDecisionResult FromAccessState(SetupAccessState accessState, bool isSetupCompleted)
    {
        var allowMainApplication = accessState == SetupAccessState.MainApplicationAccessible;
        var recommendedRedirectPath = allowMainApplication ? MainApplicationRoute : SetupWizardRoute;

        return new SetupGuardDecisionResult(
            AccessState: accessState,
            IsSetupCompleted: isSetupCompleted,
            AllowSetupWizard: true,
            AllowMainApplication: allowMainApplication,
            RecommendedRedirectPath: recommendedRedirectPath);
    }
}
