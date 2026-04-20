namespace QPhising.Application.Contracts.Abstractions.Reporting;

public interface ITrackingReportExporter
{
    byte[] BuildCsv(TrackingReportData data, string language);
    byte[] BuildPdf(TrackingReportData data, string language);
}
