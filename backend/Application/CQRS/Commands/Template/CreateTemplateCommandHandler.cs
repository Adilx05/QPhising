using MediatR;
using QPhising.Application.Contracts.Abstractions.Template;
using QPhising.Application.Contracts.Responses.Template;
using QPhising.Domain.Templates.Aggregates;
using QPhising.Domain.Templates.ValueObjects;

namespace QPhising.Application.CQRS.Commands.Template;

public sealed class CreateTemplateCommandHandler : IRequestHandler<CreateTemplateCommand, TemplateResult>
{
    private readonly ITemplateRepository _templateRepository;

    public CreateTemplateCommandHandler(ITemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<TemplateResult> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        var aggregate = new TemplateAggregate(
            id: Guid.NewGuid(),
            name: new TemplateName(request.Name),
            content: new TemplateContent(request.Subject, request.Body),
            metadata: new TemplateMetadata(request.Description, request.Tags));

        await _templateRepository.SaveAsync(aggregate, cancellationToken);

        return TemplateResult.FromAggregate(aggregate);
    }
}
