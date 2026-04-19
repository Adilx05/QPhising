using FluentValidation;
using QPhising.Application.Contracts.Abstractions.Tracking;

namespace QPhising.Application.CQRS.Queries.Tracking;

public sealed class GetTrackingAnalyticsOverviewQueryValidator : AbstractValidator<GetTrackingAnalyticsOverviewQuery>
{
    public GetTrackingAnalyticsOverviewQueryValidator()
    {
        RuleFor(query => query.TrendWindow)
            .IsInEnum()
            .WithMessage("Trend window must be hour, day, or week.");

        RuleFor(query => query.TimezoneOffsetMinutes)
            .InclusiveBetween(-840, 840)
            .WithMessage("Timezone offset minutes must be between -840 and 840.");

        RuleFor(query => query.TopPagesLimit)
            .InclusiveBetween(1, 50)
            .WithMessage("Top pages limit must be between 1 and 50.");

        RuleFor(query => query.RecentVisitLimit)
            .InclusiveBetween(1, 250)
            .WithMessage("Recent visit limit must be between 1 and 250.");

        RuleFor(query => query)
            .Must(query => !query.FromUtc.HasValue || !query.ToUtc.HasValue || query.FromUtc <= query.ToUtc)
            .WithMessage("FromUtc must be less than or equal to ToUtc.");
    }
}
