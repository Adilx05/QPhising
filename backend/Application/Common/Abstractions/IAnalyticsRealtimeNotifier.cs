namespace QPhising.Application.Common.Abstractions;

public interface IAnalyticsRealtimeNotifier
{
    Task PublishDashboardUpdatedAsync(AnalyticsDashboardUpdatedEvent updateEvent, CancellationToken cancellationToken = default);
}

public sealed record AnalyticsDashboardUpdatedEvent(
    string Source,
    Guid? CampaignId,
    DateTimeOffset OccurredAtUtc);

