using AutoMapper;
using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Campaigns.Exceptions;

namespace QPhising.Application.Features.Campaigns.ScheduleCampaign;

public sealed class ScheduleCampaignCommandHandler(
    ICampaignRepository campaignRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IAnalyticsDashboardCache analyticsDashboardCache) : IRequestHandler<ScheduleCampaignCommand, Result<ScheduleCampaignResponse>>
{
    public async Task<Result<ScheduleCampaignResponse>> Handle(ScheduleCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await campaignRepository.GetByIdAsync(request.CampaignId, cancellationToken);
        if (campaign is null)
        {
            return Result<ScheduleCampaignResponse>.Failure($"Campaign '{request.CampaignId}' was not found.");
        }

        try
        {
            campaign.Schedule();
        }
        catch (CampaignDomainException exception)
        {
            return Result<ScheduleCampaignResponse>.Failure(exception.Message);
        }

        campaignRepository.Update(campaign);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await analyticsDashboardCache.InvalidateAsync(cancellationToken);

        return Result<ScheduleCampaignResponse>.Success(mapper.Map<ScheduleCampaignResponse>(campaign));
    }
}
