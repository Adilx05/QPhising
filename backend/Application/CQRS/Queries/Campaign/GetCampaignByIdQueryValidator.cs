using FluentValidation;

namespace QPhising.Application.CQRS.Queries.Campaign;

public sealed class GetCampaignByIdQueryValidator : AbstractValidator<GetCampaignByIdQuery>
{
    public GetCampaignByIdQueryValidator()
    {
        RuleFor(query => query.CampaignId)
            .NotEqual(Guid.Empty)
            .WithMessage("Campaign ID is required.");
    }
}
