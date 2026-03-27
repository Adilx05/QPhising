using AutoMapper;
using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Campaigns.Exceptions;

namespace QPhising.Application.Features.Campaigns.ActivateCampaign;

public sealed class ActivateCampaignCommandHandler(
    ICampaignRepository campaignRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<ActivateCampaignCommand, Result<ActivateCampaignResponse>>
{
    public async Task<Result<ActivateCampaignResponse>> Handle(ActivateCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await campaignRepository.GetByIdAsync(request.CampaignId, cancellationToken);
        if (campaign is null)
        {
            return Result<ActivateCampaignResponse>.Failure($"Campaign '{request.CampaignId}' was not found.");
        }

        try
        {
            campaign.Activate();
        }
        catch (CampaignDomainException exception)
        {
            return Result<ActivateCampaignResponse>.Failure(exception.Message);
        }

        campaignRepository.Update(campaign);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ActivateCampaignResponse>.Success(mapper.Map<ActivateCampaignResponse>(campaign));
    }
}
