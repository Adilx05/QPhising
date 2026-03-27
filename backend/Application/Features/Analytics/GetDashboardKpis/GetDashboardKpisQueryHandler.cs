using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions;
using QPhising.Domain.Campaigns;

namespace QPhising.Application.Features.Analytics.GetDashboardKpis;

public sealed class GetDashboardKpisQueryHandler(
    IAnalyticsReadRepository analyticsReadRepository,
    IAnalyticsDashboardCache analyticsDashboardCache) : IRequestHandler<GetDashboardKpisQuery, Result<DashboardKpisResponse>>
{
    public async Task<Result<DashboardKpisResponse>> Handle(GetDashboardKpisQuery request, CancellationToken cancellationToken)
    {
        DashboardKpisResponse? cachedResponse = await analyticsDashboardCache.GetAsync(request, cancellationToken);
        if (cachedResponse is not null)
        {
            return Result<DashboardKpisResponse>.Success(cachedResponse);
        }

        IReadOnlyCollection<Guid> campaignIds = request.CampaignIds?.Distinct().ToArray() ?? Array.Empty<Guid>();
        IReadOnlyCollection<TemplateType> templateTypes = request.TemplateTypes?.Distinct().ToArray() ?? Array.Empty<TemplateType>();
        IReadOnlyCollection<CampaignStatus> campaignStatuses = request.CampaignStatuses?.Distinct().ToArray() ?? Array.Empty<CampaignStatus>();

        AnalyticsReadCriteria criteria = new(
            request.From,
            request.To,
            campaignIds,
            templateTypes,
            campaignStatuses);

        AnalyticsDashboardReadModel readModel = await analyticsReadRepository.GetDashboardReadModelAsync(criteria, cancellationToken);

        AnalyticsFilterDimensions filters = new(
            request.From,
            request.To,
            request.TimeGrain,
            request.TimeZone,
            campaignIds,
            templateTypes,
            campaignStatuses);

        ClickKpiSummary clickSummary = new(
            Total: readModel.ClickTotal,
            Unique: readModel.ClickUnique,
            ClickThroughRatePercent: CalculateRate(readModel.ClickUnique, readModel.CampaignTotal),
            DeltaPercent: 0m);

        ConversionKpiSummary conversionSummary = new(
            Total: readModel.ConversionTotal,
            ConversionRatePercent: CalculateRate(readModel.ConversionTotal, readModel.ClickUnique),
            DeltaPercent: 0m);

        TaskThroughputKpiSummary taskSummary = new(
            Enqueued: readModel.TasksEnqueued,
            Processed: readModel.TasksProcessed,
            Succeeded: readModel.TasksSucceeded,
            Failed: readModel.TasksFailed,
            Retried: readModel.TasksRetried,
            DeadLettered: readModel.TasksDeadLettered,
            SuccessRatePercent: CalculateRate(readModel.TasksSucceeded, readModel.TasksProcessed),
            AverageDurationMilliseconds: readModel.AverageTaskDurationMilliseconds);

        TimeZoneInfo timeZone = ResolveTimeZone(request.TimeZone);
        IReadOnlyCollection<AnalyticsTrendPoint> trend = readModel.Trend
            .GroupBy(point => ResolveBucketStart(point.BucketStartUtc, request.TimeGrain, timeZone))
            .OrderBy(group => group.Key)
            .Select(group => new AnalyticsTrendPoint(
                group.Key,
                group.Sum(point => point.Clicks),
                group.Sum(point => point.Conversions),
                group.Sum(point => point.TasksProcessed),
                CalculateRate(group.Sum(point => point.Conversions), group.Sum(point => point.Clicks))))
            .ToArray();

        IReadOnlyCollection<CampaignStatusBreakdownItem> campaignStatusBreakdown = readModel.CampaignStatusBreakdown
            .OrderBy(item => item.Status)
            .Select(item => new CampaignStatusBreakdownItem(
                item.Status,
                item.CampaignCount,
                item.ClickCount,
                item.ConversionCount))
            .ToArray();

        IReadOnlyCollection<TemplateTypeBreakdownItem> templateTypeBreakdown = readModel.TemplateTypeBreakdown
            .OrderBy(item => item.TemplateType)
            .Select(item => new TemplateTypeBreakdownItem(
                item.TemplateType,
                item.CampaignCount,
                item.ClickCount,
                item.ConversionCount,
                CalculateRate(item.ConversionCount, item.ClickCount)))
            .ToArray();

        DashboardKpisResponse response = new(
            Filters: filters,
            Campaigns: new CampaignKpiSummary(
                readModel.CampaignTotal,
                readModel.CampaignDraft,
                readModel.CampaignScheduled,
                readModel.CampaignActive,
                readModel.CampaignEnded,
                readModel.CampaignArchived),
            Clicks: clickSummary,
            Conversions: conversionSummary,
            TaskThroughput: taskSummary,
            Trend: trend,
            CampaignStatusBreakdown: campaignStatusBreakdown,
            TemplateTypeBreakdown: templateTypeBreakdown);

        await analyticsDashboardCache.SetAsync(request, response, cancellationToken);
        return Result<DashboardKpisResponse>.Success(response);
    }

    private static decimal CalculateRate(long numerator, long denominator)
    {
        if (denominator <= 0 || numerator <= 0)
        {
            return 0m;
        }

        return decimal.Round((decimal)numerator / denominator * 100m, 2, MidpointRounding.AwayFromZero);
    }

    private static decimal CalculateRate(int numerator, int denominator)
    {
        if (denominator <= 0 || numerator <= 0)
        {
            return 0m;
        }

        return decimal.Round((decimal)numerator / denominator * 100m, 2, MidpointRounding.AwayFromZero);
    }

    private static TimeZoneInfo ResolveTimeZone(string requestedTimeZone)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(requestedTimeZone);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Utc;
        }
    }

    private static DateTimeOffset ResolveBucketStart(
        DateTimeOffset bucketStartUtc,
        AnalyticsTimeGrain grain,
        TimeZoneInfo timeZone)
    {
        DateTimeOffset local = TimeZoneInfo.ConvertTime(bucketStartUtc, timeZone);

        DateTimeOffset normalizedLocal = grain switch
        {
            AnalyticsTimeGrain.Hour => new DateTimeOffset(local.Year, local.Month, local.Day, local.Hour, 0, 0, local.Offset),
            AnalyticsTimeGrain.Day => new DateTimeOffset(local.Year, local.Month, local.Day, 0, 0, 0, local.Offset),
            AnalyticsTimeGrain.Week => StartOfWeek(local),
            AnalyticsTimeGrain.Month => new DateTimeOffset(local.Year, local.Month, 1, 0, 0, 0, local.Offset),
            _ => new DateTimeOffset(local.Year, local.Month, local.Day, 0, 0, 0, local.Offset)
        };

        return normalizedLocal.ToUniversalTime();
    }

    private static DateTimeOffset StartOfWeek(DateTimeOffset local)
    {
        int distanceFromMonday = ((int)local.DayOfWeek + 6) % 7;
        DateTimeOffset monday = local.AddDays(-distanceFromMonday);
        return new DateTimeOffset(monday.Year, monday.Month, monday.Day, 0, 0, 0, monday.Offset);
    }
}
