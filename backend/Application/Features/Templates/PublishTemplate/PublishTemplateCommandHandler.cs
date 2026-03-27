using AutoMapper;
using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Templates.Exceptions;

namespace QPhising.Application.Features.Templates.PublishTemplate;

public sealed class PublishTemplateCommandHandler(
    ITemplateRepository templateRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<PublishTemplateCommand, Result<PublishTemplateResponse>>
{
    public async Task<Result<PublishTemplateResponse>> Handle(PublishTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template is null)
        {
            return Result<PublishTemplateResponse>.Failure($"Template '{request.TemplateId}' was not found.");
        }

        try
        {
            template.Publish();
        }
        catch (TemplateDomainException exception)
        {
            return Result<PublishTemplateResponse>.Failure(exception.Message);
        }

        templateRepository.Update(template);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        PublishTemplateResponse response = mapper.Map<PublishTemplateResponse>(template);
        return Result<PublishTemplateResponse>.Success(response);
    }
}
