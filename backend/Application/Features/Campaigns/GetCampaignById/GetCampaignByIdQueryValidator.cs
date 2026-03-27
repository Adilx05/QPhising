using FluentValidation;

namespace QPhising.Application.Features.Campaigns.GetCampaignById;

public sealed class GetCampaignByIdQueryValidator : AbstractValidator<GetCampaignByIdQuery>
{
    public GetCampaignByIdQueryValidator()
    {
        RuleFor(query => query.CampaignId)
            .NotEmpty();
    }
}
