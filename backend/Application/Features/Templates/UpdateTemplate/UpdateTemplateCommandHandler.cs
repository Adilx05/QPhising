using AutoMapper;
using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Templates.Exceptions;

namespace QPhising.Application.Features.Templates.UpdateTemplate;

public sealed class UpdateTemplateCommandHandler(
    ITemplateRepository templateRepository,
    IUnitOfWork unitOfWork,
    ITemplateHtmlSanitizer templateHtmlSanitizer,
    IMapper mapper) : IRequestHandler<UpdateTemplateCommand, Result<UpdateTemplateResponse>>
{
    public async Task<Result<UpdateTemplateResponse>> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template is null)
        {
            return Result<UpdateTemplateResponse>.Failure($"Template '{request.TemplateId}' was not found.");
        }

        try
        {
            Result<string> sanitizeResult = templateHtmlSanitizer.Sanitize(request.HtmlContent);
            if (!sanitizeResult.IsSuccess || sanitizeResult.Value is null)
            {
                return Result<UpdateTemplateResponse>.Failure(sanitizeResult.Errors.ToArray());
            }

            template.UpdateContent(
                request.Name,
                request.Type,
                sanitizeResult.Value,
                request.Variables);
        }
        catch (TemplateDomainException exception)
        {
            return Result<UpdateTemplateResponse>.Failure(exception.Message);
        }

        templateRepository.Update(template);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        UpdateTemplateResponse response = mapper.Map<UpdateTemplateResponse>(template);
        return Result<UpdateTemplateResponse>.Success(response);
    }
}
