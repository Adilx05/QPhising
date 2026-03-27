using FluentValidation;

namespace QPhising.Application.Features.Exports.DownloadExportFile;

public sealed class DownloadExportFileQueryValidator : AbstractValidator<DownloadExportFileQuery>
{
    public DownloadExportFileQueryValidator()
    {
        RuleFor(query => query.ExportJobId)
            .NotEmpty();

        RuleFor(query => query.RequestingUserId)
            .NotEmpty()
            .MaximumLength(200);
    }
}
