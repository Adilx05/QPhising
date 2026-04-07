using FluentValidation;

namespace QPhising.Application.Features.Setup.GetSetupStatus;

public sealed class GetSetupStatusQueryValidator : AbstractValidator<GetSetupStatusQuery>
{
    public GetSetupStatusQueryValidator()
    {
    }
}
