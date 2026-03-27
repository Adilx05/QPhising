using QPhising.Application.Common.Abstractions;

namespace QPhising.Application.Common;

public sealed class NoOpAnalyticsRealtimeNotifier : IAnalyticsRealtimeNotifier
{
    public Task PublishDashboardUpdatedAsync(AnalyticsDashboardUpdatedEvent updateEvent, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

