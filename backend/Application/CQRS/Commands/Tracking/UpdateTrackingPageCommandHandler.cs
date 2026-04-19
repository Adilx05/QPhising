using MediatR;
using QPhising.Application.Contracts.Abstractions.Template;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Application.Contracts.Responses.Tracking;
using QPhising.Application.Mapping.Tracking;
using QPhising.Domain.Tracking.Models;
using QPhising.Domain.Tracking.ValueObjects;

namespace QPhising.Application.CQRS.Commands.Tracking;

public sealed class UpdateTrackingPageCommandHandler : IRequestHandler<UpdateTrackingPageCommand, TrackingPageResult>
{
    private readonly ITrackingPageRepository _trackingPageRepository;
    private readonly ITemplateRepository _templateRepository;

    public UpdateTrackingPageCommandHandler(ITrackingPageRepository trackingPageRepository, ITemplateRepository templateRepository)
    {
        _trackingPageRepository = trackingPageRepository;
        _templateRepository = templateRepository;
    }

    public async Task<TrackingPageResult> Handle(UpdateTrackingPageCommand request, CancellationToken cancellationToken)
    {
        if (await _trackingPageRepository.SlugExistsAsync(request.Slug, request.TrackingPageId, cancellationToken))
        {
            throw new InvalidOperationException($"Tracking page slug '{request.Slug}' already exists.");
        }

        var aggregate = await _trackingPageRepository.GetByIdAsync(request.TrackingPageId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tracking page '{request.TrackingPageId}' was not found.");

        var templateId = await ResolveTemplateIdAsync(request.TemplateId, cancellationToken);

        aggregate.UpdateDetails(
            new TrackingPageSlug(request.Slug),
            request.Title,
            request.Description,
            new TrackingPageUrl(request.DestinationUrl),
            templateId);

        aggregate.ConfigureSettings(BuildSettings(request.RetentionDays, request.MaskIpAddress, request.EnableBotFiltering, request.CaptureUtmParameters));

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
