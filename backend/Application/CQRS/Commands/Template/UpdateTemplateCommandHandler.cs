using MediatR;
using QPhising.Application.Contracts.Abstractions.Template;
using QPhising.Application.Contracts.Responses.Template;
using QPhising.Domain.Templates.ValueObjects;

namespace QPhising.Application.CQRS.Commands.Template;

public sealed class UpdateTemplateCommandHandler : IRequestHandler<UpdateTemplateCommand, TemplateResult>
{
    private readonly ITemplateRepository _templateRepository;

    public UpdateTemplateCommandHandler(ITemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<TemplateResult> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        var aggregate = await _templateRepository.GetByIdAsync(request.TemplateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Template '{request.TemplateId}' was not found.");

        aggregate.Update(
            name: new TemplateName(request.Name),
            content: new TemplateContent(request.HtmlContent),
            metadata: new TemplateMetadata(request.Description, request.Tags));

        await _templateRepository.SaveAsync(aggregate, cancellationToken);

        return TemplateResult.FromAggregate(aggregate);
    }
}
