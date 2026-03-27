using QPhising.Application.Features.Analytics.GetDashboardKpis;
using QPhising.Domain.Campaigns;

namespace QPhising.Application.Common.Abstractions;

public interface IAnalyticsReadRepository
{
    Task<AnalyticsDashboardReadModel> GetDashboardReadModelAsync(AnalyticsReadCriteria criteria, CancellationToken cancellationToken = default);
}

public sealed record AnalyticsReadCriteria(
    DateTimeOffset From,
    DateTimeOffset To,
    IReadOnlyCollection<Guid> CampaignIds,
    IReadOnlyCollection<TemplateType> TemplateTypes,
    IReadOnlyCollection<CampaignStatus> CampaignStatuses);

public sealed record AnalyticsDashboardReadModel(
    int CampaignTotal,
    int CampaignDraft,
    int CampaignScheduled,
    int CampaignActive,
    int CampaignEnded,
    int CampaignArchived,
    long ClickTotal,
    long ClickUnique,
    long ConversionTotal,
    long TasksEnqueued,
    long TasksProcessed,
    long TasksSucceeded,
    long TasksFailed,
    long TasksRetried,
    long TasksDeadLettered,
    decimal AverageTaskDurationMilliseconds,
    IReadOnlyCollection<AnalyticsTrendReadModel> Trend,
    IReadOnlyCollection<CampaignStatusBreakdownReadModel> CampaignStatusBreakdown,
    IReadOnlyCollection<TemplateTypeBreakdownReadModel> TemplateTypeBreakdown);

public sealed record AnalyticsTrendReadModel(
    DateTimeOffset BucketStartUtc,
    long Clicks,
    long Conversions,
    long TasksProcessed);

public sealed record CampaignStatusBreakdownReadModel(
    CampaignStatus Status,
    int CampaignCount,
    long ClickCount,
    long ConversionCount);

public sealed record TemplateTypeBreakdownReadModel(
    TemplateType TemplateType,
    int CampaignCount,
    long ClickCount,
    long ConversionCount);
