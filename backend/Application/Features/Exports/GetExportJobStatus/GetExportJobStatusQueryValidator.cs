using FluentValidation;

namespace QPhising.Application.Features.Exports.GetExportJobStatus;

public sealed class GetExportJobStatusQueryValidator : AbstractValidator<GetExportJobStatusQuery>
{
    public GetExportJobStatusQueryValidator()
    {
        RuleFor(query => query.ExportJobId)
            .NotEmpty();

        RuleFor(query => query.RequestingUserId)
            .NotEmpty()
            .MaximumLength(200);
    }
}
