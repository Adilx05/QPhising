using FluentValidation;

namespace QPhising.Application.CQRS.Queries.Campaign;

public sealed class ListCampaignsQueryValidator : AbstractValidator<ListCampaignsQuery>
{
    public ListCampaignsQueryValidator()
    {
        RuleFor(query => query)
            .NotNull();
    }
}
