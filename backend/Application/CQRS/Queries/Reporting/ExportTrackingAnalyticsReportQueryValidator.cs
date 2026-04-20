using FluentValidation;
using QPhising.Application.Contracts.Abstractions.Reporting;

namespace QPhising.Application.CQRS.Queries.Reporting;

public sealed class ExportTrackingAnalyticsReportQueryValidator : AbstractValidator<ExportTrackingAnalyticsReportQuery>
{
    public ExportTrackingAnalyticsReportQueryValidator()
    {
        RuleFor(query => query.TimezoneOffsetMinutes)
            .InclusiveBetween(-840, 840);

        RuleFor(query => query)
            .Must(query => !query.FromUtc.HasValue || !query.ToUtc.HasValue || query.FromUtc <= query.ToUtc)
            .WithMessage("fromUtc must be less than or equal to toUtc.");

        RuleFor(query => query.TrackingPageId)
            .NotEmpty()
            .When(query => query.Scope == TrackingReportScope.TrackingPage)
            .WithMessage("trackingPageId is required for tracking-page scope exports.");
    }
}
