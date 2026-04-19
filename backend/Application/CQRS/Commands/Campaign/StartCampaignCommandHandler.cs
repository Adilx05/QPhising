using MediatR;
using QPhising.Application.Contracts.Abstractions.Campaign;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Application.Contracts.Responses.Campaign;
using QPhising.Domain.Tracking.Enums;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed class StartCampaignCommandHandler : IRequestHandler<StartCampaignCommand, CampaignResult>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ITrackingPageRepository _trackingPageRepository;

    public StartCampaignCommandHandler(ICampaignRepository campaignRepository, ITrackingPageRepository trackingPageRepository)
    {
        _campaignRepository = campaignRepository;
        _trackingPageRepository = trackingPageRepository;
    }

    public async Task<CampaignResult> Handle(StartCampaignCommand request, CancellationToken cancellationToken)
    {
        var aggregate = await _campaignRepository.GetByIdAsync(request.CampaignId, cancellationToken)
            ?? throw new KeyNotFoundException($"Campaign '{request.CampaignId}' was not found.");

        aggregate.Start();

        var trackingPage = await _trackingPageRepository.GetByIdAsync(aggregate.TrackingPageId, cancellationToken);
        if (trackingPage is not null && trackingPage.PublishState != TrackingPagePublishState.Published)
        {
            trackingPage.Publish();
            await _trackingPageRepository.SaveAsync(trackingPage, cancellationToken);
        }

        await _campaignRepository.SaveAsync(aggregate, cancellationToken);

        return CampaignResult.FromAggregate(aggregate);
    }
}
