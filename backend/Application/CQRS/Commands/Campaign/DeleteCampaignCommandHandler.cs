using MediatR;
using QPhising.Application.Contracts.Abstractions.Campaign;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed class DeleteCampaignCommandHandler : IRequestHandler<DeleteCampaignCommand, Unit>
{
    private readonly ICampaignRepository _campaignRepository;

    public DeleteCampaignCommandHandler(ICampaignRepository campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }

    public async Task<Unit> Handle(DeleteCampaignCommand request, CancellationToken cancellationToken)
    {
        var aggregate = await _campaignRepository.GetByIdAsync(request.CampaignId, cancellationToken)
            ?? throw new KeyNotFoundException($"Campaign '{request.CampaignId}' was not found.");

        await _campaignRepository.DeleteAsync(aggregate, cancellationToken);

        return Unit.Value;
    }
}
