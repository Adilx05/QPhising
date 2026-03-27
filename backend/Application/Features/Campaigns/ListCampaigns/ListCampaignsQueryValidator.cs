using FluentValidation;

namespace QPhising.Application.Features.Campaigns.ListCampaigns;

public sealed class ListCampaignsQueryValidator : AbstractValidator<ListCampaignsQuery>
{
    private const int MaxTake = 200;

    public ListCampaignsQueryValidator()
    {
        RuleFor(query => query.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(query => query.Take)
            .InclusiveBetween(1, MaxTake);

        RuleFor(query => query)
            .Must(query => query.StartsOnOrAfter is null || query.EndsOnOrBefore is null || query.StartsOnOrAfter <= query.EndsOnOrBefore)
            .WithMessage("StartsOnOrAfter must be less than or equal to EndsOnOrBefore when both are provided.");
    }
}
