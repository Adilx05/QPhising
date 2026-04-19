using FluentValidation;
using QPhising.Domain.Tracking.ValueObjects;

namespace QPhising.Application.CQRS.Queries.Tracking;

public sealed class GetTrackingLandingPageBySlugQueryValidator : AbstractValidator<GetTrackingLandingPageBySlugQuery>
{
    public GetTrackingLandingPageBySlugQueryValidator()
    {
        RuleFor(query => query.Slug)
            .NotEmpty()
            .MaximumLength(TrackingPageSlug.MaxLength);
    }
}
