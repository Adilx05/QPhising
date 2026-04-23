using MediatR;
using QPhising.Application.Contracts.Abstractions.Campaign;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Domain.Campaign.Enums;

namespace QPhising.Application.CQRS.Commands.Tracking;

public sealed class DeleteTrackingPageCommandHandler : IRequestHandler<DeleteTrackingPageCommand, Unit>
{
    private readonly ITrackingPageRepository _trackingPageRepository;
    private readonly ICampaignRepository _campaignRepository;

    public DeleteTrackingPageCommandHandler(
        ITrackingPageRepository trackingPageRepository,
        ICampaignRepository campaignRepository)
    {
        _trackingPageRepository = trackingPageRepository;
        _campaignRepository = campaignRepository;
    }

    public async Task<Unit> Handle(DeleteTrackingPageCommand request, CancellationToken cancellationToken)
    {
        var aggregate = await _trackingPageRepository.GetByIdAsync(request.TrackingPageId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tracking page '{request.TrackingPageId}' was not found.");

        var campaign = await _campaignRepository.GetByTrackingPageIdAsync(aggregate.Id, cancellationToken);
        if (campaign is not null && campaign.LifecycleState is not (CampaignLifecycleState.Cancelled or CampaignLifecycleState.Completed))
        {
            campaign.Cancel();
            await _campaignRepository.SaveAsync(campaign, cancellationToken);
        }

        aggregate.MarkDeleted();
        await _trackingPageRepository.DeleteAsync(aggregate, cancellationToken);

        return Unit.Value;
    }
}
