using AutoMapper;
using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Abstractions;

namespace QPhising.Application.Features.Campaigns.UpdateCampaign;

public sealed class UpdateCampaignCommandHandler(
    ICampaignRepository campaignRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<UpdateCampaignCommand, Result<UpdateCampaignResponse>>
{
    public async Task<Result<UpdateCampaignResponse>> Handle(UpdateCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await campaignRepository.GetByIdAsync(request.CampaignId, cancellationToken);
        if (campaign is null)
        {
            return Result<UpdateCampaignResponse>.Failure($"Campaign '{request.CampaignId}' was not found.");
        }

        campaign.UpdateDetails(
            request.Name,
            request.TemplateType,
            request.HtmlContent,
            request.StartDate,
            request.EndDate);

        campaignRepository.Update(campaign);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        UpdateCampaignResponse response = mapper.Map<UpdateCampaignResponse>(campaign);
        return Result<UpdateCampaignResponse>.Success(response);
    }
}
