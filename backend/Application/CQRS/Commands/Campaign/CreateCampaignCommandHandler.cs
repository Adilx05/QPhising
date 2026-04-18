using MediatR;
using QPhising.Application.Contracts.Abstractions.Campaign;
using QPhising.Application.Contracts.Responses.Campaign;
using QPhising.Domain.Campaign.Aggregates;
using QPhising.Domain.Campaign.ValueObjects;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed class CreateCampaignCommandHandler : IRequestHandler<CreateCampaignCommand, CampaignResult>
{
    private readonly ICampaignRepository _campaignRepository;

    public CreateCampaignCommandHandler(ICampaignRepository campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }

    public async Task<CampaignResult> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        var aggregate = new CampaignAggregate(Guid.NewGuid(), new CampaignName(request.Name), request.TemplateId);
        await _campaignRepository.SaveAsync(aggregate, cancellationToken);
        return CampaignResult.FromAggregate(aggregate);
    }
}
