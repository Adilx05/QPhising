using Microsoft.EntityFrameworkCore;
using QPhising.Application.Common.Abstractions;
using QPhising.Domain.Campaigns;
using QPhising.Domain.Tasks;

namespace QPhising.Infrastructure.Persistence.Repositories;

public sealed class AnalyticsReadRepository(QPhisingDbContext dbContext) : IAnalyticsReadRepository
{
    public async Task<AnalyticsDashboardReadModel> GetDashboardReadModelAsync(AnalyticsReadCriteria criteria, CancellationToken cancellationToken = default)
    {
        IQueryable<Campaign> campaignsQuery = dbContext.Campaigns.AsNoTracking()
            .Where(campaign => campaign.StartDate <= criteria.To && campaign.EndDate >= criteria.From);

        if (criteria.CampaignIds.Count > 0)
        {
            campaignsQuery = campaignsQuery.Where(campaign => criteria.CampaignIds.Contains(campaign.Id));
        }

        if (criteria.TemplateTypes.Count > 0)
        {
            campaignsQuery = campaignsQuery.Where(campaign => criteria.TemplateTypes.Contains(campaign.TemplateType));
        }

        if (criteria.CampaignStatuses.Count > 0)
        {
            campaignsQuery = campaignsQuery.Where(campaign => criteria.CampaignStatuses.Contains(campaign.Status));
        }

        CampaignSlice[] campaignRows = await campaignsQuery
            .Select(campaign => new CampaignSlice(campaign.Id, campaign.Status, campaign.TemplateType))
            .ToArrayAsync(cancellationToken);

        HashSet<Guid> filteredCampaignIds = campaignRows.Select(row => row.Id).ToHashSet();

        IQueryable<Domain.Tracking.TrackingClick> clicksQuery = dbContext.TrackingClicks.AsNoTracking()
            .Where(click => click.ClickedAtUtc >= criteria.From && click.ClickedAtUtc <= criteria.To);

        if (filteredCampaignIds.Count > 0)
        {
            clicksQuery = clicksQuery.Where(click => filteredCampaignIds.Contains(click.CampaignId));
        }

        TrackingClickSlice[] clickRows = await clicksQuery
            .Select(click => new TrackingClickSlice(click.CampaignId, click.ClickedAtUtc, click.TrackingToken))
            .ToArrayAsync(cancellationToken);

        IQueryable<QueuedTask> queuedTasksQuery = dbContext.QueuedTasks.AsNoTracking()
            .Where(task => task.CreatedAt >= criteria.From && task.CreatedAt <= criteria.To);

        var rawTaskRows = await queuedTasksQuery
            .Select(task => new { task.Id, task.Type, task.CreatedAt, task.Payload })
            .ToArrayAsync(cancellationToken);

        TaskSlice[] taskRows = rawTaskRows
            .Select(task => new TaskSlice(task.Id, task.Type, task.CreatedAt, task.Payload.Values))
            .ToArray();

        HashSet<Guid> taskIds = taskRows.Select(task => task.Id).ToHashSet();

        IQueryable<TaskExecutionLog> taskLogsQuery = dbContext.TaskExecutionLogs.AsNoTracking()
            .Where(log => log.OccurredAt >= criteria.From && log.OccurredAt <= criteria.To);

        if (taskIds.Count > 0)
        {
            taskLogsQuery = taskLogsQuery.Where(log => taskIds.Contains(log.TaskId));
        }

        TaskLogSlice[] taskLogRows = await taskLogsQuery
            .Select(log => new TaskLogSlice(log.TaskId, log.EventType, log.OccurredAt, log.ExecutionDurationMilliseconds))
            .ToArrayAsync(cancellationToken);

        Dictionary<Guid, long> clickCountByCampaign = clickRows
            .GroupBy(click => click.CampaignId)
            .ToDictionary(group => group.Key, group => (long)group.Count());

        Dictionary<Guid, long> uniqueClickCountByCampaign = clickRows
            .GroupBy(click => click.CampaignId)
            .ToDictionary(
                group => group.Key,
                group => (long)group.Select(click => click.TrackingToken).Distinct(StringComparer.Ordinal).Count());

        HashSet<Guid> succeededTrackingTaskIds = taskLogRows
            .Where(log => log.EventType == TaskExecutionLogEventType.Succeeded)
            .Select(log => log.TaskId)
            .ToHashSet();

        Dictionary<Guid, long> conversionCountByCampaign = taskRows
            .Where(task => task.Type == TaskType.TrackingClickProcessing && succeededTrackingTaskIds.Contains(task.Id))
            .Select(task => TryResolveCampaignId(task.PayloadValues))
            .Where(campaignId => campaignId.HasValue)
            .GroupBy(campaignId => campaignId!.Value)
            .ToDictionary(group => group.Key, group => (long)group.Count());

        int campaignTotal = campaignRows.Length;
        long clickTotal = clickRows.LongLength;
        long clickUnique = clickRows.Select(click => click.TrackingToken).Distinct(StringComparer.Ordinal).LongCount();
        long conversionTotal = conversionCountByCampaign.Values.Sum();

        long tasksEnqueued = taskRows.LongLength;
        long tasksProcessed = taskLogRows.LongCount(log => log.EventType is TaskExecutionLogEventType.Succeeded or TaskExecutionLogEventType.Failed);
        long tasksSucceeded = taskLogRows.LongCount(log => log.EventType == TaskExecutionLogEventType.Succeeded);
        long tasksFailed = taskLogRows.LongCount(log => log.EventType == TaskExecutionLogEventType.Failed);
        long tasksRetried = taskLogRows.LongCount(log => log.EventType == TaskExecutionLogEventType.Retried);
        long tasksDeadLettered = taskLogRows.LongCount(log => log.EventType == TaskExecutionLogEventType.DeadLettered);

        decimal averageDuration = taskLogRows
            .Where(log => log.ExecutionDurationMilliseconds.HasValue)
            .Select(log => log.ExecutionDurationMilliseconds!.Value)
            .DefaultIfEmpty(0)
            .Average();

        IReadOnlyCollection<AnalyticsTrendReadModel> trend = BuildTrend(criteria, clickRows, taskRows, taskLogRows, succeededTrackingTaskIds);

        IReadOnlyCollection<CampaignStatusBreakdownReadModel> statusBreakdown = campaignRows
            .GroupBy(row => row.Status)
            .Select(group => new CampaignStatusBreakdownReadModel(
                group.Key,
                group.Count(),
                group.Sum(campaign => clickCountByCampaign.GetValueOrDefault(campaign.Id)),
                group.Sum(campaign => conversionCountByCampaign.GetValueOrDefault(campaign.Id))))
            .ToArray();

        IReadOnlyCollection<TemplateTypeBreakdownReadModel> templateBreakdown = campaignRows
            .GroupBy(row => row.TemplateType)
            .Select(group => new TemplateTypeBreakdownReadModel(
                group.Key,
                group.Count(),
                group.Sum(campaign => clickCountByCampaign.GetValueOrDefault(campaign.Id)),
                group.Sum(campaign => conversionCountByCampaign.GetValueOrDefault(campaign.Id))))
            .ToArray();

        return new AnalyticsDashboardReadModel(
            CampaignTotal: campaignTotal,
            CampaignDraft: campaignRows.Count(row => row.Status == CampaignStatus.Draft),
            CampaignScheduled: campaignRows.Count(row => row.Status == CampaignStatus.Scheduled),
            CampaignActive: campaignRows.Count(row => row.Status == CampaignStatus.Active),
            CampaignEnded: campaignRows.Count(row => row.Status == CampaignStatus.Ended),
            CampaignArchived: campaignRows.Count(row => row.Status == CampaignStatus.Archived),
            ClickTotal: clickTotal,
            ClickUnique: clickUnique,
            ConversionTotal: conversionTotal,
            TasksEnqueued: tasksEnqueued,
            TasksProcessed: tasksProcessed,
            TasksSucceeded: tasksSucceeded,
            TasksFailed: tasksFailed,
            TasksRetried: tasksRetried,
            TasksDeadLettered: tasksDeadLettered,
            AverageTaskDurationMilliseconds: decimal.Round(averageDuration, 2, MidpointRounding.AwayFromZero),
            Trend: trend,
            CampaignStatusBreakdown: statusBreakdown,
            TemplateTypeBreakdown: templateBreakdown);
    }

    private static IReadOnlyCollection<AnalyticsTrendReadModel> BuildTrend(
        AnalyticsReadCriteria criteria,
        IReadOnlyCollection<TrackingClickSlice> clicks,
        IReadOnlyCollection<TaskSlice> tasks,
        IReadOnlyCollection<TaskLogSlice> taskLogs,
        IReadOnlySet<Guid> succeededTrackingTaskIds)
    {
        Dictionary<DateTimeOffset, long> clickBuckets = clicks
            .GroupBy(click => RoundDownToHour(click.ClickedAtUtc))
            .ToDictionary(group => group.Key, group => (long)group.Count());

        Dictionary<DateTimeOffset, long> taskProcessedBuckets = taskLogs
            .Where(log => log.EventType is TaskExecutionLogEventType.Succeeded or TaskExecutionLogEventType.Failed)
            .GroupBy(log => RoundDownToHour(log.OccurredAt))
            .ToDictionary(group => group.Key, group => (long)group.Count());

        Dictionary<DateTimeOffset, long> conversionBuckets = tasks
            .Where(task => task.Type == TaskType.TrackingClickProcessing && succeededTrackingTaskIds.Contains(task.Id))
            .GroupBy(task => RoundDownToHour(task.CreatedAt))
            .ToDictionary(group => group.Key, group => (long)group.Count());

        DateTimeOffset current = RoundDownToHour(criteria.From);
        DateTimeOffset end = RoundDownToHour(criteria.To);
        List<AnalyticsTrendReadModel> trend = [];

        while (current <= end)
        {
            trend.Add(new AnalyticsTrendReadModel(
                current,
                clickBuckets.GetValueOrDefault(current),
                conversionBuckets.GetValueOrDefault(current),
                taskProcessedBuckets.GetValueOrDefault(current)));

            current = current.AddHours(1);
        }

        return trend;
    }

    private static DateTimeOffset RoundDownToHour(DateTimeOffset value) =>
        new(value.Year, value.Month, value.Day, value.Hour, 0, 0, TimeSpan.Zero);

    private static Guid? TryResolveCampaignId(IReadOnlyDictionary<string, string> payloadValues)
    {
        if (!payloadValues.TryGetValue("campaignId", out string? campaignIdValue))
        {
            return null;
        }

        return Guid.TryParse(campaignIdValue, out Guid campaignId)
            ? campaignId
            : null;
    }

    private sealed record CampaignSlice(Guid Id, CampaignStatus Status, TemplateType TemplateType);
    private sealed record TrackingClickSlice(Guid CampaignId, DateTimeOffset ClickedAtUtc, string TrackingToken);
    private sealed record TaskSlice(Guid Id, TaskType Type, DateTimeOffset CreatedAt, IReadOnlyDictionary<string, string> PayloadValues);
    private sealed record TaskLogSlice(Guid TaskId, TaskExecutionLogEventType EventType, DateTimeOffset OccurredAt, long? ExecutionDurationMilliseconds);
}
