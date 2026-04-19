using MediatR;
using QPhising.Application.Contracts.Abstractions.Authorization;
using QPhising.Application.Contracts.Abstractions.Template;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Application.Contracts.Responses.Tracking;
using QPhising.Application.Mapping.Tracking;
using QPhising.Domain.Tracking.Aggregates;
using QPhising.Domain.Tracking.Models;
using QPhising.Domain.Tracking.ValueObjects;

namespace QPhising.Application.CQRS.Commands.Tracking;

public sealed class CreateTrackingPageCommandHandler : IRequestHandler<CreateTrackingPageCommand, TrackingPageResult>
{
    private readonly ITrackingPageRepository _trackingPageRepository;
    private readonly ITemplateRepository _templateRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public CreateTrackingPageCommandHandler(
        ITrackingPageRepository trackingPageRepository,
        ITemplateRepository templateRepository,
        ICurrentUserContext currentUserContext)
    {
        _trackingPageRepository = trackingPageRepository;
        _templateRepository = templateRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<TrackingPageResult> Handle(CreateTrackingPageCommand request, CancellationToken cancellationToken)
    {
        if (await _trackingPageRepository.SlugExistsAsync(request.Slug, null, cancellationToken))
        {
            throw new InvalidOperationException($"Tracking page slug '{request.Slug}' already exists.");
        }

        var templateId = await ResolveTemplateIdAsync(request.TemplateId, cancellationToken);
        var ownerId = ResolveOwnerId(request.OwnerId);
        var settings = BuildSettings(request.RetentionDays, request.MaskIpAddress, request.EnableBotFiltering, request.CaptureUtmParameters);

        var aggregate = new TrackingPageAggregate(
            Guid.NewGuid(),
            new TrackingPageSlug(request.Slug),
            request.Title,
            request.Description,
            new TrackingPageUrl(request.DestinationUrl),
            ownerId,
            templateId,
            settings);

        await _trackingPageRepository.SaveAsync(aggregate, cancellationToken);

        return aggregate.ToResult();
    }

    private async Task<Guid?> ResolveTemplateIdAsync(Guid? templateId, CancellationToken cancellationToken)
    {
        if (!templateId.HasValue || templateId.Value == Guid.Empty)
        {
            return null;
        }

        var template = await _templateRepository.GetByIdAsync(templateId.Value, cancellationToken);
        if (template is null)
        {
            throw new KeyNotFoundException($"Template '{templateId.Value}' was not found.");
        }

        return template.Id;
    }

    private string ResolveOwnerId(string? requestOwnerId)
    {
        if (!string.IsNullOrWhiteSpace(requestOwnerId))
        {
            return requestOwnerId;
        }

        if (!string.IsNullOrWhiteSpace(_currentUserContext.UserId))
        {
            return _currentUserContext.UserId;
        }

        throw new InvalidOperationException("Tracking page owner could not be resolved from request or authenticated user context.");
    }

    private static PageSettings? BuildSettings(int? retentionDays, bool? maskIpAddress, bool? enableBotFiltering, bool? captureUtmParameters)
    {
        if (!retentionDays.HasValue && !maskIpAddress.HasValue && !enableBotFiltering.HasValue && !captureUtmParameters.HasValue)
        {
            return null;
        }

        return new PageSettings(
            retentionDays ?? 365,
            maskIpAddress ?? true,
            enableBotFiltering ?? true,
            captureUtmParameters ?? true);
    }
}
