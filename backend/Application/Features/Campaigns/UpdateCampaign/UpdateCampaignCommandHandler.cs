using AutoMapper;
using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions;
using QPhising.Domain.Abstractions;

namespace QPhising.Application.Features.Campaigns.UpdateCampaign;

public sealed class UpdateCampaignCommandHandler(
    ICampaignRepository campaignRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IAnalyticsDashboardCache analyticsDashboardCache,
    IAnalyticsRealtimeNotifier analyticsRealtimeNotifier) : IRequestHandler<UpdateCampaignCommand, Result<UpdateCampaignResponse>>
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
        await analyticsDashboardCache.InvalidateAsync(cancellationToken);
        await analyticsRealtimeNotifier.PublishDashboardUpdatedAsync(
            new AnalyticsDashboardUpdatedEvent("campaign_updated", campaign.Id, DateTimeOffset.UtcNow),
            cancellationToken);

        UpdateCampaignResponse response = mapper.Map<UpdateCampaignResponse>(campaign);
        return Result<UpdateCampaignResponse>.Success(response);
    }
}
