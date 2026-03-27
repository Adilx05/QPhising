using AutoMapper;
using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Templates.Exceptions;

namespace QPhising.Application.Features.Templates.ArchiveTemplate;

public sealed class ArchiveTemplateCommandHandler(
    ITemplateRepository templateRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<ArchiveTemplateCommand, Result<ArchiveTemplateResponse>>
{
    public async Task<Result<ArchiveTemplateResponse>> Handle(ArchiveTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template is null)
        {
            return Result<ArchiveTemplateResponse>.Failure($"Template '{request.TemplateId}' was not found.");
        }

        try
        {
            template.Archive();
        }
        catch (TemplateDomainException exception)
        {
            return Result<ArchiveTemplateResponse>.Failure(exception.Message);
        }

        templateRepository.Update(template);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ArchiveTemplateResponse response = mapper.Map<ArchiveTemplateResponse>(template);
        return Result<ArchiveTemplateResponse>.Success(response);
    }
}
