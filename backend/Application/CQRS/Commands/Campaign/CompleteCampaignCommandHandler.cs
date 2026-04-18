using MediatR;
using QPhising.Application.Contracts.Abstractions.Campaign;
using QPhising.Application.Contracts.Responses.Campaign;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed class CompleteCampaignCommandHandler : IRequestHandler<CompleteCampaignCommand, CampaignResult>
{
    private readonly ICampaignRepository _campaignRepository;

    public CompleteCampaignCommandHandler(ICampaignRepository campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }

    public async Task<CampaignResult> Handle(CompleteCampaignCommand request, CancellationToken cancellationToken)
    {
        var aggregate = await _campaignRepository.GetByIdAsync(request.CampaignId, cancellationToken)
            ?? throw new KeyNotFoundException($"Campaign '{request.CampaignId}' was not found.");

        aggregate.Complete();

        await _campaignRepository.SaveAsync(aggregate, cancellationToken);

        return CampaignResult.FromAggregate(aggregate);
    }
}
