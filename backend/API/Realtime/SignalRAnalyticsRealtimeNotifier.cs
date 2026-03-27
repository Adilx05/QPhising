using Microsoft.AspNetCore.SignalR;
using QPhising.Application.Common.Abstractions;

namespace QPhising.API.Realtime;

public sealed class SignalRAnalyticsRealtimeNotifier(IHubContext<AnalyticsHub> hubContext) : IAnalyticsRealtimeNotifier
{
    public Task PublishDashboardUpdatedAsync(AnalyticsDashboardUpdatedEvent updateEvent, CancellationToken cancellationToken = default)
    {
        return hubContext.Clients.All.SendAsync("analytics.dashboard.updated", updateEvent, cancellationToken);
    }
}

