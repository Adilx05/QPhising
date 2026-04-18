using MediatR;
using QPhising.Application.Contracts.Abstractions.Template;
using QPhising.Application.Contracts.Responses.Template;

namespace QPhising.Application.CQRS.Commands.Template;

public sealed class PublishTemplateCommandHandler : IRequestHandler<PublishTemplateCommand, TemplateResult>
{
    private readonly ITemplateRepository _templateRepository;

    public PublishTemplateCommandHandler(ITemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<TemplateResult> Handle(PublishTemplateCommand request, CancellationToken cancellationToken)
    {
        var aggregate = await _templateRepository.GetByIdAsync(request.TemplateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Template '{request.TemplateId}' was not found.");

        aggregate.Publish();

        await _templateRepository.SaveAsync(aggregate, cancellationToken);

        return TemplateResult.FromAggregate(aggregate);
    }
}
