using System;

using QPhising.Domain.Campaign.Enums;

namespace QPhising.Domain.Campaign.Policies;

public static class CampaignLifecyclePolicy
{
    public static bool CanTransition(CampaignLifecycleState currentState, CampaignLifecycleState nextState) =>
        currentState switch
        {
            CampaignLifecycleState.Draft => nextState is CampaignLifecycleState.Scheduled or CampaignLifecycleState.Active or CampaignLifecycleState.Cancelled,
            CampaignLifecycleState.Scheduled => nextState is CampaignLifecycleState.Active or CampaignLifecycleState.Cancelled,
            CampaignLifecycleState.Active => nextState is CampaignLifecycleState.Paused or CampaignLifecycleState.Completed or CampaignLifecycleState.Cancelled,
            CampaignLifecycleState.Paused => nextState is CampaignLifecycleState.Active or CampaignLifecycleState.Cancelled,
            CampaignLifecycleState.Completed => false,
            CampaignLifecycleState.Cancelled => false,
            _ => false
        };

    public static void EnsureTransitionAllowed(CampaignLifecycleState currentState, CampaignLifecycleState nextState)
    {
        if (currentState == nextState)
        {
            return;
        }

        if (!CanTransition(currentState, nextState))
        {
            throw new InvalidOperationException($"Campaign cannot transition from {currentState} to {nextState}.");
        }
    }
}
