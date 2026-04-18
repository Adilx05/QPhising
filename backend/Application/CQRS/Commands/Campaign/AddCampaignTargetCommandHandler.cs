using MediatR;
using QPhising.Application.Contracts.Abstractions.Campaign;
using QPhising.Application.Contracts.Responses.Campaign;
using QPhising.Domain.Campaign.Entities;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed class AddCampaignTargetCommandHandler : IRequestHandler<AddCampaignTargetCommand, CampaignResult>
{
    private readonly ICampaignRepository _campaignRepository;

    public AddCampaignTargetCommandHandler(ICampaignRepository campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }

    public async Task<CampaignResult> Handle(AddCampaignTargetCommand request, CancellationToken cancellationToken)
    {
        var aggregate = await _campaignRepository.GetByIdAsync(request.CampaignId, cancellationToken)
            ?? throw new KeyNotFoundException($"Campaign '{request.CampaignId}' was not found.");

        aggregate.AddTarget(new CampaignTarget(Guid.NewGuid(), request.EmailAddress));

        await _campaignRepository.SaveAsync(aggregate, cancellationToken);

        return CampaignResult.FromAggregate(aggregate);
    }
}
