using System.Globalization;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using QPhising.Application.Contracts.Abstractions.Reporting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace QPhising.Api.Services.Reporting;

public sealed class TrackingReportExporter : ITrackingReportExporter
{
    private readonly IHostEnvironment _hostEnvironment;
    private readonly TrackingReportBrandingOptions _branding;

    public TrackingReportExporter(
        IHostEnvironment hostEnvironment,
        IOptions<TrackingReportBrandingOptions> brandingOptions)
    {
        _hostEnvironment = hostEnvironment;
        _branding = brandingOptions.Value;
    }

    public byte[] BuildCsv(TrackingReportData data, string language)
    {
        var isTr = IsTurkish(language);

        var builder = new StringBuilder();
        builder.AppendLine("Section,Label,Value,Extra");
        builder.AppendLine($"Metadata,Title,{Escape(isTr ? "Takip Analiz Raporu" : "Tracking Analytics Report")},");
        builder.AppendLine($"Metadata,Scope,{Escape(data.ScopeLabel)},");
        builder.AppendLine($"Metadata,RangeFrom,{Escape(data.FromUtc?.ToString("O") ?? "all-time")},");
        builder.AppendLine($"Metadata,RangeTo,{Escape(data.ToUtc?.ToString("O") ?? "now")},");
        builder.AppendLine($"Summary,{Escape(isTr ? "Toplam Ziyaret" : "Total Visits")},{data.TotalVisits},");
        builder.AppendLine($"Summary,{Escape(isTr ? "Benzersiz Ziyaretçi" : "Unique Visitors")},{data.UniqueVisitors},");
        builder.AppendLine($"Summary,{Escape(isTr ? "Son Ziyaret" : "Last Visit")},{Escape(data.LastVisitAtUtc?.ToString("O") ?? "-")},");

        foreach (var note in data.AppliedNotes)
            builder.AppendLine($"Notes,{Escape(isTr ? "Not" : "Note")},{Escape(note)},");

        foreach (var trend in data.TrendRows)
            builder.AppendLine($"Trend,{Escape(trend.Label)},{trend.TotalVisits},{trend.UniqueVisitors}");

        foreach (var row in data.DistributionRows)
            builder.AppendLine($"Distribution,{Escape(row.Label)},{row.Value},");

        foreach (var visitor in data.VisitorRows)
            builder.AppendLine($"Visitors,{Escape(visitor.VisitorKey)},{visitor.ClickCount},{Escape(visitor.LastOccurredAtUtc.ToString("O"))}");

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    public byte[] BuildPdf(TrackingReportData data, string language)
    {
        var document = new TrackingAnalyticsPdfDocument(
            data,
            NormalizeLanguage(language),
            ResolveLogoBytes(),
            _branding);

        return document.GeneratePdf();
    }

    private byte[]? ResolveLogoBytes()
    {
        if (string.IsNullOrWhiteSpace(_branding.LogoPath))
            return null;

        var fullPath = Path.IsPathRooted(_branding.LogoPath)
            ? _branding.LogoPath
            : Path.Combine(_hostEnvironment.ContentRootPath, _branding.LogoPath);

        return File.Exists(fullPath) ? File.ReadAllBytes(fullPath) : null;
    }

    private static bool IsTurkish(string? language) =>
        !string.IsNullOrWhiteSpace(language) &&
        language.StartsWith("tr", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeLanguage(string? language) =>
        IsTurkish(language) ? "tr" : "en";

    private static string Escape(string value)
    {
        var normalized = value.Replace("\"", "\"\"");
        return $"\"{normalized}\"";
    }
}
