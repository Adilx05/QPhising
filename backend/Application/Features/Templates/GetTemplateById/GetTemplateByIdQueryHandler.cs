using AutoMapper;
using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Abstractions;

namespace QPhising.Application.Features.Templates.GetTemplateById;

public sealed class GetTemplateByIdQueryHandler(
    ITemplateRepository templateRepository,
    IMapper mapper) : IRequestHandler<GetTemplateByIdQuery, Result<TemplateDetailResponse>>
{
    public async Task<Result<TemplateDetailResponse>> Handle(GetTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template is null)
        {
            return Result<TemplateDetailResponse>.Failure($"Template '{request.TemplateId}' was not found.");
        }

        TemplateDetailResponse response = mapper.Map<TemplateDetailResponse>(template);
        return Result<TemplateDetailResponse>.Success(response);
    }
}
