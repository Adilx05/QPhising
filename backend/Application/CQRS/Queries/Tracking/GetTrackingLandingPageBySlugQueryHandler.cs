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
        var aggregate = await ResolveAggregateAsync(request, cancellationToken)
            ?? throw new KeyNotFoundException($"Tracking page slug '{request.Slug}' was not found.");

        if (!aggregate.IsPubliclyAccessible(DateTimeOffset.UtcNow))
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
            aggregate.TemplateId,
            templateName,
            templateHtmlContent,
            aggregate.CustomHtmlContent,
            aggregate.ValidFromUtc,
            aggregate.ValidUntilUtc,
            aggregate.Settings?.CaptureIpAddress ?? true,
            aggregate.Settings?.IpAddressHashPolicy ?? IpAddressHashPolicy.Sha256);
    }

    private async Task<QPhising.Domain.Tracking.Aggregates.TrackingPageAggregate?> ResolveAggregateAsync(GetTrackingLandingPageBySlugQuery request, CancellationToken cancellationToken)
    {
        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            var byId = await _trackingPageRepository.GetByIdAsync(request.Id.Value, cancellationToken);
            if (byId is not null && byId.Slug.Value.Equals(request.Slug.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return byId;
            }
        }

        return await _trackingPageRepository.GetBySlugAsync(request.Slug, cancellationToken);
    }
}
