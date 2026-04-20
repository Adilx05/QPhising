namespace QPhising.Application.Contracts.Abstractions.Reporting;

public interface ITrackingReportExporter
{
    byte[] BuildCsv(TrackingReportData data);

    byte[] BuildPdf(TrackingReportData data);
}
