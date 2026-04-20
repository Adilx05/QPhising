namespace QPhising.Application.Contracts.Responses.Reporting;

public sealed record TrackingReportFileResult(
    string ContentType,
    string FileName,
    byte[] Content);
