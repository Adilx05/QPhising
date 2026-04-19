using FluentValidation;

namespace QPhising.Application.CQRS.Queries.Tracking;

public sealed class GetTrackingPageByIdQueryValidator : AbstractValidator<GetTrackingPageByIdQuery>
{
    public GetTrackingPageByIdQueryValidator()
    {
        RuleFor(query => query.TrackingPageId)
            .NotEqual(Guid.Empty)
            .WithMessage("Tracking page ID is required.");
    }
}
