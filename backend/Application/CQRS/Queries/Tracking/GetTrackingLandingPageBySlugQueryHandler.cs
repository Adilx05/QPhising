using MediatR;
using QPhising.Application.Contracts.Abstractions.Template;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Application.Contracts.Responses.Tracking;
using QPhising.Domain.Templates.Enums;
using QPhising.Domain.Tracking.Enums;

namespace QPhising.Application.CQRS.Queries.Tracking;

public sealed class GetTrackingLandingPageBySlugQueryHandler : IRequestHandler<GetTrackingLandingPageBySlugQuery, TrackingLandingPageResult>
{
    private readonly ITrackingPageRepository _trackingPageRepository;
    private readonly ITemplateRepository _templateRepository;

    public GetTrackingLandingPageBySlugQueryHandler(ITrackingPageRepository trackingPageRepository, ITemplateRepository templateRepository)
    {
        _trackingPageRepository = trackingPageRepository;
        _templateRepository = templateRepository;
    }

    public async Task<TrackingLandingPageResult> Handle(GetTrackingLandingPageBySlugQuery request, CancellationToken cancellationToken)
    {
        var aggregate = await _trackingPageRepository.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException($"Tracking page slug '{request.Slug}' was not found.");

        if (aggregate.PublishState != TrackingPagePublishState.Published)
        {
            throw new KeyNotFoundException($"Tracking page slug '{request.Slug}' was not found.");
        }

        string? templateName = null;
        string? templateHtmlContent = null;

        if (aggregate.TemplateId.HasValue)
        {
            var template = await _templateRepository.GetByIdAsync(aggregate.TemplateId.Value, cancellationToken);
            if (template is not null && template.LifecycleState is not TemplateLifecycleState.Archived)
            {
                templateName = template.Name.Value;
                templateHtmlContent = template.Content.HtmlContent;
            }
        }

        return new TrackingLandingPageResult(
            aggregate.Id,
            aggregate.Slug.Value,
            aggregate.Title,
            aggregate.Description,
            aggregate.DestinationUrl.Value,
            aggregate.TemplateId,
            templateName,
            templateHtmlContent);
    }
}
