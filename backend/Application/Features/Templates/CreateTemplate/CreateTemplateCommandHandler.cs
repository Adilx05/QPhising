using AutoMapper;
using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Templates;
using QPhising.Domain.Templates.Exceptions;

namespace QPhising.Application.Features.Templates.CreateTemplate;

public sealed class CreateTemplateCommandHandler(
    ITemplateRepository templateRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<CreateTemplateCommand, Result<CreateTemplateResponse>>
{
    public async Task<Result<CreateTemplateResponse>> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        Template template;

        try
        {
            template = Template.Create(
                request.Name,
                request.Type,
                request.HtmlContent,
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
