using QPhising.Domain.Campaign.Aggregates;
using QPhising.Domain.Campaign.Enums;

namespace QPhising.Application.Contracts.Responses.Campaign;

public sealed record CampaignResult(
    Guid Id,
    string Name,
    Guid TemplateId,
    CampaignLifecycleState LifecycleState,
    DateTimeOffset? StartsAtUtc,
    DateTimeOffset? EndsAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyCollection<CampaignTargetResult> Targets)
{
    public static CampaignResult FromAggregate(CampaignAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        var targets = aggregate.Targets
            .Select(target => new CampaignTargetResult(target.Id, target.EmailAddress))
            .ToArray();

        return new CampaignResult(
            Id: aggregate.Id,
            Name: aggregate.Name.Value,
            TemplateId: aggregate.TemplateId,
            LifecycleState: aggregate.LifecycleState,
            StartsAtUtc: aggregate.ScheduleWindow?.StartsAtUtc,
            EndsAtUtc: aggregate.ScheduleWindow?.EndsAtUtc,
            CreatedAtUtc: aggregate.CreatedAtUtc,
            UpdatedAtUtc: aggregate.UpdatedAtUtc,
            Targets: targets);
    }
}
