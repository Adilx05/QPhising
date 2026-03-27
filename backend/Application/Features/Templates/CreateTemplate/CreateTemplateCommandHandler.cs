using AutoMapper;
using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Templates;
using QPhising.Domain.Templates.Exceptions;

namespace QPhising.Application.Features.Templates.CreateTemplate;

public sealed class CreateTemplateCommandHandler(
    ITemplateRepository templateRepository,
    IUnitOfWork unitOfWork,
    ITemplateHtmlSanitizer templateHtmlSanitizer,
    IMapper mapper) : IRequestHandler<CreateTemplateCommand, Result<CreateTemplateResponse>>
{
    public async Task<Result<CreateTemplateResponse>> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        Result<string> sanitizeResult = templateHtmlSanitizer.Sanitize(request.HtmlContent);
        if (!sanitizeResult.IsSuccess || sanitizeResult.Value is null)
        {
            return Result<CreateTemplateResponse>.Failure(sanitizeResult.Errors.ToArray());
        }

        Template template;

        try
        {
            template = Template.Create(
                request.Name,
                request.Type,
                sanitizeResult.Value,
                request.Variables);
        }
        catch (TemplateDomainException exception)
        {
            return Result<CreateTemplateResponse>.Failure(exception.Message);
        }

        await templateRepository.AddAsync(template, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        CreateTemplateResponse response = mapper.Map<CreateTemplateResponse>(template);
        return Result<CreateTemplateResponse>.Success(response);
    }
}
