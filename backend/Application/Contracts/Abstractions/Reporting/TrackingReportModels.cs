namespace QPhising.Application.Contracts.Abstractions.Reporting;

public enum TrackingReportFormat
{
    Csv = 0,
    Pdf = 1
}

public enum TrackingReportScope
{
    Global = 0,
    TrackingPage = 1
}

public enum TrackingReportDetailLevel
{
    Summary = 0,
    Detailed = 1
}

public sealed record TrackingVisitorClickStat(
    string VisitorKey,
    string? SessionId,
    string? VisitorFingerprint,
    string? IpHash,
    int ClickCount,
    DateTimeOffset LastOccurredAtUtc);

public sealed record TrackingReportData(
    string Title,
    string ScopeLabel,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    int TotalVisits,
    int UniqueVisitors,
    DateTimeOffset? LastVisitAtUtc,
    IReadOnlyCollection<(string Label, int TotalVisits, int UniqueVisitors)> TrendRows,
    IReadOnlyCollection<(string Label, int Value)> DistributionRows,
    IReadOnlyCollection<TrackingVisitorClickStat> VisitorRows,
    IReadOnlyCollection<string> AppliedNotes);
