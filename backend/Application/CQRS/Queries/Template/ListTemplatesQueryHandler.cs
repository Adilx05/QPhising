using MediatR;
using QPhising.Application.Contracts.Abstractions.Template;
using QPhising.Application.Contracts.Responses.Template;

namespace QPhising.Application.CQRS.Queries.Template;

public sealed class ListTemplatesQueryHandler : IRequestHandler<ListTemplatesQuery, IReadOnlyCollection<TemplateResult>>
{
    private readonly ITemplateRepository _templateRepository;

    public ListTemplatesQueryHandler(ITemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<IReadOnlyCollection<TemplateResult>> Handle(ListTemplatesQuery request, CancellationToken cancellationToken)
    {
        var templates = await _templateRepository.ListAsync(cancellationToken);
        return templates.Select(TemplateResult.FromAggregate).ToArray();
    }
}
