using AutoMapper;
using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Campaigns;

namespace QPhising.Application.Features.Campaigns.CreateCampaign;

public sealed class CreateCampaignCommandHandler(
    ICampaignRepository campaignRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IAnalyticsDashboardCache analyticsDashboardCache) : IRequestHandler<CreateCampaignCommand, Result<CreateCampaignResponse>>
{
    public async Task<Result<CreateCampaignResponse>> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        Campaign campaign = Campaign.Create(
            request.Name,
            request.TemplateType,
            request.HtmlContent,
            request.StartDate,
            request.EndDate);

        await campaignRepository.AddAsync(campaign, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await analyticsDashboardCache.InvalidateAsync(cancellationToken);

        CreateCampaignResponse response = mapper.Map<CreateCampaignResponse>(campaign);
        return Result<CreateCampaignResponse>.Success(response);
    }
}
