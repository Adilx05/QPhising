using MediatR;
using QPhising.Application.Contracts.Abstractions.Template;

namespace QPhising.Application.CQRS.Commands.Template;

public sealed class DeleteTemplateCommandHandler : IRequestHandler<DeleteTemplateCommand, Unit>
{
    private readonly ITemplateRepository _templateRepository;

    public DeleteTemplateCommandHandler(ITemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<Unit> Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
    {
        var aggregate = await _templateRepository.GetByIdAsync(request.TemplateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Template '{request.TemplateId}' was not found.");

        aggregate.MarkDeleted();
        await _templateRepository.DeleteAsync(aggregate, cancellationToken);

        return Unit.Value;
    }
}
