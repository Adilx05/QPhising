using MediatR;
using QPhising.Application.Contracts.Abstractions.Campaign;
using QPhising.Application.Contracts.Abstractions.Tracking;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed class DeleteCampaignCommandHandler : IRequestHandler<DeleteCampaignCommand, Unit>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ITrackingPageRepository _trackingPageRepository;

    public DeleteCampaignCommandHandler(
        ICampaignRepository campaignRepository,
        ITrackingPageRepository trackingPageRepository)
    {
        _campaignRepository = campaignRepository;
        _trackingPageRepository = trackingPageRepository;
    }

    public async Task<Unit> Handle(DeleteCampaignCommand request, CancellationToken cancellationToken)
    {
        var aggregate = await _campaignRepository.GetByIdAsync(request.CampaignId, cancellationToken)
            ?? throw new KeyNotFoundException($"Campaign '{request.CampaignId}' was not found.");

        var trackingPage = await _trackingPageRepository.GetByIdAsync(aggregate.TrackingPageId, cancellationToken);
        if (trackingPage is not null)
        {
            trackingPage.MarkDeleted();
            await _trackingPageRepository.DeleteAsync(trackingPage, cancellationToken);
        }

        aggregate.MarkDeleted();
        await _campaignRepository.DeleteAsync(aggregate, cancellationToken);

        return Unit.Value;
    }
}
