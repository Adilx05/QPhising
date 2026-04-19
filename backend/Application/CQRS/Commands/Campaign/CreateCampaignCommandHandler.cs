using MediatR;
using QPhising.Application.Contracts.Abstractions.Authorization;
using QPhising.Application.Contracts.Abstractions.Campaign;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Application.Contracts.Responses.Campaign;
using QPhising.Domain.Campaign.Aggregates;
using QPhising.Domain.Campaign.ValueObjects;
using QPhising.Domain.Tracking.Aggregates;
using QPhising.Domain.Tracking.Enums;
using QPhising.Domain.Tracking.Models;
using QPhising.Domain.Tracking.ValueObjects;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed class CreateCampaignCommandHandler : IRequestHandler<CreateCampaignCommand, CampaignResult>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ITrackingPageRepository _trackingPageRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public CreateCampaignCommandHandler(
        ICampaignRepository campaignRepository,
        ITrackingPageRepository trackingPageRepository,
        ICurrentUserContext currentUserContext)
    {
        _campaignRepository = campaignRepository;
        _trackingPageRepository = trackingPageRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<CampaignResult> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        var slug = new TrackingPageSlug(request.TrackingPageSlug);
        var slugExists = await _trackingPageRepository.SlugExistsAsync(slug.Value, excludingTrackingPageId: null, cancellationToken);
        if (slugExists)
        {
            throw new InvalidOperationException($"Tracking page slug '{slug.Value}' is already in use.");
        }

        var ownerId = string.IsNullOrWhiteSpace(_currentUserContext.UserId) ? "campaign-operator" : _currentUserContext.UserId;
        var trackingPage = new TrackingPageAggregate(
            Guid.NewGuid(),
            slug,
            request.TrackingPageTitle,
            request.TrackingPageDescription,
            ownerId!,
            request.TemplateId,
            request.HtmlContent,
            request.ValidFromUtc,
            request.ValidUntilUtc);
        trackingPage.ConfigureSettings(new PageSettings(
            retentionDays: 365,
            captureIpAddress: true,
            ipAddressHashPolicy: IpAddressHashPolicy.Sha256,
            enableBotFiltering: true,
            captureUtmParameters: true));
        trackingPage.Publish();

        await _trackingPageRepository.SaveAsync(trackingPage, cancellationToken);

        var aggregate = new CampaignAggregate(Guid.NewGuid(), new CampaignName(request.Name), trackingPage.Id, request.TemplateId);
        await _campaignRepository.SaveAsync(aggregate, cancellationToken);
        return CampaignResult.FromAggregate(aggregate);
    }
}
