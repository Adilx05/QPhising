using QPhising.Application.Features.Analytics.GetDashboardKpis;

namespace QPhising.Application.Common.Abstractions;

public interface IAnalyticsDashboardCache
{
    Task<DashboardKpisResponse?> GetAsync(GetDashboardKpisQuery query, CancellationToken cancellationToken = default);

    Task SetAsync(
        GetDashboardKpisQuery query,
        DashboardKpisResponse response,
        CancellationToken cancellationToken = default);

    Task InvalidateAsync(CancellationToken cancellationToken = default);
}
