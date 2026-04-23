using FluentValidation;

namespace QPhising.Application.CQRS.Queries.Audit;

public sealed class ListAuditLogsQueryValidator : AbstractValidator<ListAuditLogsQuery>
{
    public ListAuditLogsQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThan(0);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 200);

        RuleFor(query => query.Actor)
            .MaximumLength(128)
            .When(query => !string.IsNullOrWhiteSpace(query.Actor));

        RuleFor(query => query.Endpoint)
            .MaximumLength(512)
            .When(query => !string.IsNullOrWhiteSpace(query.Endpoint));

        RuleFor(query => query.CorrelationId)
            .MaximumLength(128)
            .When(query => !string.IsNullOrWhiteSpace(query.CorrelationId));

        RuleFor(query => query.OutcomeCode)
            .InclusiveBetween(100, 599)
            .When(query => query.OutcomeCode.HasValue);

        RuleFor(query => query)
            .Must(query => !query.FromUtc.HasValue || !query.ToUtc.HasValue || query.FromUtc <= query.ToUtc)
            .WithMessage("fromUtc must be less than or equal to toUtc.");
    }
}
