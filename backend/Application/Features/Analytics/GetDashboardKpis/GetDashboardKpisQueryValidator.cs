using FluentValidation;

namespace QPhising.Application.Features.Analytics.GetDashboardKpis;

public sealed class GetDashboardKpisQueryValidator : AbstractValidator<GetDashboardKpisQuery>
{
    private const int MaximumWindowDays = 366;
    private const int MaximumFilterItems = 100;

    public GetDashboardKpisQueryValidator()
    {
        RuleFor(query => query.From)
            .LessThan(query => query.To)
            .WithMessage("From must be earlier than To.");

        RuleFor(query => query)
            .Must(query => query.To - query.From <= TimeSpan.FromDays(MaximumWindowDays))
            .WithMessage($"Date window cannot exceed {MaximumWindowDays} days.");

        RuleFor(query => query.TimeZone)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(query => query.CampaignIds)
            .Must(ids => ids is null || ids.Count <= MaximumFilterItems)
            .WithMessage($"CampaignIds cannot contain more than {MaximumFilterItems} values.");

        RuleFor(query => query.TemplateTypes)
            .Must(types => types is null || types.Count <= MaximumFilterItems)
            .WithMessage($"TemplateTypes cannot contain more than {MaximumFilterItems} values.");

        RuleFor(query => query.CampaignStatuses)
            .Must(statuses => statuses is null || statuses.Count <= MaximumFilterItems)
            .WithMessage($"CampaignStatuses cannot contain more than {MaximumFilterItems} values.");
    }
}
