using FluentValidation;

namespace QPhising.Application.CQRS.Queries.Tracking;

public sealed class GetTrackingPageAnalyticsQueryValidator : AbstractValidator<GetTrackingPageAnalyticsQuery>
{
    public GetTrackingPageAnalyticsQueryValidator()
    {
        RuleFor(query => query.TrackingPageId)
            .NotEqual(Guid.Empty)
            .WithMessage("Tracking page ID is required.");

        RuleFor(query => query.TrendBucketSizeMinutes)
            .InclusiveBetween(5, 1440)
            .WithMessage("Trend bucket size must be between 5 and 1440 minutes.");

        RuleFor(query => query.RecentVisitLimit)
            .InclusiveBetween(1, 250)
            .WithMessage("Recent visit limit must be between 1 and 250.");

        RuleFor(query => query)
            .Must(query => !query.FromUtc.HasValue || !query.ToUtc.HasValue || query.FromUtc <= query.ToUtc)
            .WithMessage("FromUtc must be less than or equal to ToUtc.");
    }
}
