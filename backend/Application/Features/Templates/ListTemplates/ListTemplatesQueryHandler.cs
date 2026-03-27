using AutoMapper;
using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Abstractions;

namespace QPhising.Application.Features.Templates.ListTemplates;

public sealed class ListTemplatesQueryHandler(
    ITemplateRepository templateRepository,
    IMapper mapper) : IRequestHandler<ListTemplatesQuery, Result<ListTemplatesResponse>>
{
    public async Task<Result<ListTemplatesResponse>> Handle(ListTemplatesQuery request, CancellationToken cancellationToken)
    {
        TemplateReadCriteria criteria = new(
            request.Status,
            request.Type,
            request.SearchTerm,
            request.PageNumber,
            request.PageSize);

        var templates = await templateRepository.ListAsync(criteria, cancellationToken);
        var items = mapper.Map<IReadOnlyCollection<TemplateListItemResponse>>(templates);

        ListTemplatesResponse response = new(request.PageNumber, request.PageSize, items);
        return Result<ListTemplatesResponse>.Success(response);
    }
}
