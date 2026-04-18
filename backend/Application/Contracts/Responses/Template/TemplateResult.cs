using QPhising.Domain.Templates.Aggregates;
using QPhising.Domain.Templates.Enums;

namespace QPhising.Application.Contracts.Responses.Template;

public sealed record TemplateResult(
    Guid Id,
    string Name,
    string Subject,
    string Body,
    string? Description,
    IReadOnlyCollection<string> Tags,
    TemplateLifecycleState LifecycleState,
    int Version,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc)
{
    public static TemplateResult FromAggregate(TemplateAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        return new TemplateResult(
            Id: aggregate.Id,
            Name: aggregate.Name.Value,
            Subject: aggregate.Content.Subject,
            Body: aggregate.Content.Body,
            Description: aggregate.Metadata.Description,
            Tags: aggregate.Metadata.Tags.ToArray(),
            LifecycleState: aggregate.LifecycleState,
            Version: aggregate.Version,
            CreatedAtUtc: aggregate.CreatedAtUtc,
            UpdatedAtUtc: aggregate.UpdatedAtUtc);
    }
}
