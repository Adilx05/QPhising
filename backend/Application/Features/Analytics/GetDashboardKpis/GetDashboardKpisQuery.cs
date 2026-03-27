using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Campaigns;

namespace QPhising.Application.Features.Analytics.GetDashboardKpis;

public sealed record GetDashboardKpisQuery(
    DateTimeOffset From,
    DateTimeOffset To,
    AnalyticsTimeGrain TimeGrain = AnalyticsTimeGrain.Day,
    string TimeZone = "UTC",
    IReadOnlyCollection<Guid>? CampaignIds = null,
    IReadOnlyCollection<TemplateType>? TemplateTypes = null,
    IReadOnlyCollection<CampaignStatus>? CampaignStatuses = null) : IRequest<Result<DashboardKpisResponse>>;

public enum AnalyticsTimeGrain
{
    Hour = 1,
    Day = 2,
    Week = 3,
    Month = 4
}

public sealed record DashboardKpisResponse(
    AnalyticsFilterDimensions Filters,
    CampaignKpiSummary Campaigns,
    ClickKpiSummary Clicks,
    ConversionKpiSummary Conversions,
    TaskThroughputKpiSummary TaskThroughput,
    IReadOnlyCollection<AnalyticsTrendPoint> Trend,
    IReadOnlyCollection<CampaignStatusBreakdownItem> CampaignStatusBreakdown,
    IReadOnlyCollection<TemplateTypeBreakdownItem> TemplateTypeBreakdown);

public sealed record AnalyticsFilterDimensions(
    DateTimeOffset From,
    DateTimeOffset To,
    AnalyticsTimeGrain TimeGrain,
    string TimeZone,
    IReadOnlyCollection<Guid> CampaignIds,
    IReadOnlyCollection<TemplateType> TemplateTypes,
    IReadOnlyCollection<CampaignStatus> CampaignStatuses);

public sealed record CampaignKpiSummary(
    int Total,
    int Draft,
    int Scheduled,
    int Active,
    int Ended,
    int Archived);

public sealed record ClickKpiSummary(
    long Total,
    long Unique,
    decimal ClickThroughRatePercent,
    decimal DeltaPercent);

public sealed record ConversionKpiSummary(
    long Total,
    decimal ConversionRatePercent,
    decimal DeltaPercent);

public sealed record TaskThroughputKpiSummary(
    long Enqueued,
    long Processed,
    long Succeeded,
    long Failed,
    long Retried,
    long DeadLettered,
    decimal SuccessRatePercent,
    decimal AverageDurationMilliseconds);

public sealed record AnalyticsTrendPoint(
    DateTimeOffset BucketStartUtc,
    long Clicks,
    long Conversions,
    long TasksProcessed,
    decimal ConversionRatePercent);

public sealed record CampaignStatusBreakdownItem(
    CampaignStatus Status,
    int CampaignCount,
    long ClickCount,
    long ConversionCount);

public sealed record TemplateTypeBreakdownItem(
    TemplateType TemplateType,
    int CampaignCount,
    long ClickCount,
    long ConversionCount,
    decimal ConversionRatePercent);
