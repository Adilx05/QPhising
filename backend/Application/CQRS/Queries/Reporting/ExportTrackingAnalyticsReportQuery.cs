using MediatR;
using QPhising.Application.Contracts.Abstractions.Reporting;
using QPhising.Application.Contracts.Responses.Reporting;

namespace QPhising.Application.CQRS.Queries.Reporting;

public sealed record ExportTrackingAnalyticsReportQuery(
    TrackingReportFormat Format,
    TrackingReportScope Scope,
    TrackingReportDetailLevel DetailLevel,
    Guid? TrackingPageId,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    bool ExcludeBots,
    int TimezoneOffsetMinutes) : IRequest<TrackingReportFileResult>;
