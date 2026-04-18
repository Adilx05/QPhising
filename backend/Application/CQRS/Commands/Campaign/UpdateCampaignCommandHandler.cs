using MediatR;
using QPhising.Application.Contracts.Abstractions.Campaign;
using QPhising.Application.Contracts.Responses.Campaign;
using QPhising.Domain.Campaign.ValueObjects;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed class UpdateCampaignCommandHandler : IRequestHandler<UpdateCampaignCommand, CampaignResult>
{
    private readonly ICampaignRepository _campaignRepository;

    public UpdateCampaignCommandHandler(ICampaignRepository campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }

    public async Task<CampaignResult> Handle(UpdateCampaignCommand request, CancellationToken cancellationToken)
    {
        var aggregate = await _campaignRepository.GetByIdAsync(request.CampaignId, cancellationToken)
            ?? throw new KeyNotFoundException($"Campaign '{request.CampaignId}' was not found.");

        aggregate.Rename(new CampaignName(request.Name));

        await _campaignRepository.SaveAsync(aggregate, cancellationToken);

        return CampaignResult.FromAggregate(aggregate);
    }
}
