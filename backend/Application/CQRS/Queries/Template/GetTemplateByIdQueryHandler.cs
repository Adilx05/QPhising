using MediatR;
using QPhising.Application.Contracts.Abstractions.Template;
using QPhising.Application.Contracts.Responses.Template;

namespace QPhising.Application.CQRS.Queries.Template;

public sealed class GetTemplateByIdQueryHandler : IRequestHandler<GetTemplateByIdQuery, TemplateResult>
{
    private readonly ITemplateRepository _templateRepository;

    public GetTemplateByIdQueryHandler(ITemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<TemplateResult> Handle(GetTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var aggregate = await _templateRepository.GetByIdAsync(request.TemplateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Template '{request.TemplateId}' was not found.");

        return TemplateResult.FromAggregate(aggregate);
    }
}
