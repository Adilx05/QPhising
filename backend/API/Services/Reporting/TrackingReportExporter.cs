using System.Globalization;
using System.Text;
using QPhising.Application.Contracts.Abstractions.Reporting;

namespace QPhising.Api.Services.Reporting;

public sealed class TrackingReportExporter : ITrackingReportExporter
{
    public byte[] BuildCsv(TrackingReportData data)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Section,Label,Value,Extra");
        builder.AppendLine($"Metadata,Title,{Escape(data.Title)},");
        builder.AppendLine($"Metadata,Scope,{Escape(data.ScopeLabel)},");
        builder.AppendLine($"Metadata,RangeFrom,{Escape(data.FromUtc?.ToString("O") ?? "all-time")},");
        builder.AppendLine($"Metadata,RangeTo,{Escape(data.ToUtc?.ToString("O") ?? "now")},");
        builder.AppendLine($"Summary,TotalVisits,{data.TotalVisits},");
        builder.AppendLine($"Summary,UniqueVisitors,{data.UniqueVisitors},");
        builder.AppendLine($"Summary,LastVisitAtUtc,{Escape(data.LastVisitAtUtc?.ToString("O") ?? "-")},");

        foreach (var note in data.AppliedNotes)
        {
            builder.AppendLine($"Notes,Applied,{Escape(note)},");
        }

        foreach (var trend in data.TrendRows)
        {
            builder.AppendLine($"Trend,{Escape(trend.Label)},{trend.TotalVisits},{trend.UniqueVisitors}");
        }

        foreach (var row in data.DistributionRows)
        {
            builder.AppendLine($"Distribution,{Escape(row.Label)},{row.Value},");
        }

        foreach (var visitor in data.VisitorRows)
        {
            builder.AppendLine($"Visitors,{Escape(visitor.VisitorKey)},{visitor.ClickCount},{Escape(visitor.LastOccurredAtUtc.ToString("O"))}");
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    public byte[] BuildPdf(TrackingReportData data)
    {
        var lines = BuildPdfLines(data);
        return SimplePdfWriter.CreateSinglePagePdf(lines);
    }

    private static IReadOnlyCollection<string> BuildPdfLines(TrackingReportData data)
    {
        var lines = new List<string>
        {
            data.Title,
            $"Scope: {data.ScopeLabel}",
            $"Range: {(data.FromUtc?.ToString("u", CultureInfo.InvariantCulture) ?? "all-time")} - {(data.ToUtc?.ToString("u", CultureInfo.InvariantCulture) ?? "now")}",
            string.Empty,
            $"Total Visits: {data.TotalVisits}",
            $"Unique Visitors: {data.UniqueVisitors}",
            $"Last Visit: {(data.LastVisitAtUtc?.ToString("u", CultureInfo.InvariantCulture) ?? "-")}",
            string.Empty,
            "Trend Overview"
        };

        var maxTrend = Math.Max(1, data.TrendRows.Select(static row => row.TotalVisits).DefaultIfEmpty(0).Max());
        foreach (var trend in data.TrendRows.Take(12))
        {
            var width = (int)Math.Round((trend.TotalVisits / (double)maxTrend) * 20);
            lines.Add($"{trend.Label}: {new string('█', Math.Max(1, width))} {trend.TotalVisits}");
        }

        lines.Add(string.Empty);
        lines.Add("Top Distribution");
        foreach (var row in data.DistributionRows.Take(10))
        {
            lines.Add($"{row.Label}: {row.Value}");
        }

        if (data.VisitorRows.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("Visitor Click Summary");
            foreach (var visitor in data.VisitorRows.Take(20))
            {
                lines.Add($"{visitor.VisitorKey} -> {visitor.ClickCount} clicks");
            }
        }

        return lines;
    }

    private static string Escape(string value)
    {
        var normalized = value.Replace("\"", "\"\"");
        return $"\"{normalized}\"";
    }
}

internal static class SimplePdfWriter
{
    public static byte[] CreateSinglePagePdf(IReadOnlyCollection<string> lines)
    {
        var objects = new List<string>();
        objects.Add("1 0 obj<< /Type /Catalog /Pages 2 0 R >>endobj\n");
        objects.Add("2 0 obj<< /Type /Pages /Kids [3 0 R] /Count 1 >>endobj\n");
        objects.Add("3 0 obj<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>endobj\n");

        var contentBuilder = new StringBuilder();
        contentBuilder.Append("BT\n/F1 10 Tf\n50 790 Td\n");
        var firstLine = true;
        foreach (var line in lines.Take(58))
        {
            var sanitized = SanitizePdfText(line);
            if (!firstLine)
            {
                contentBuilder.Append("0 -12 Td\n");
            }

            contentBuilder.AppendFormat(CultureInfo.InvariantCulture, "({0}) Tj\n", sanitized);
            firstLine = false;
        }

        contentBuilder.Append("ET\n");
        var content = contentBuilder.ToString();
        objects.Add($"4 0 obj<< /Length {Encoding.ASCII.GetByteCount(content)} >>stream\n{content}endstream\nendobj\n");
        objects.Add("5 0 obj<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>endobj\n");

        var output = new StringBuilder();
        output.Append("%PDF-1.4\n");

        var offsets = new List<int> { 0 };
        foreach (var item in objects)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(output.ToString()));
            output.Append(item);
        }

        var xrefStart = Encoding.ASCII.GetByteCount(output.ToString());
        output.AppendFormat(CultureInfo.InvariantCulture, "xref\n0 {0}\n", objects.Count + 1);
        output.Append("0000000000 65535 f \n");
        for (var index = 1; index < offsets.Count; index++)
        {
            output.AppendFormat(CultureInfo.InvariantCulture, "{0:0000000000} 00000 n \n", offsets[index]);
        }

        output.AppendFormat(CultureInfo.InvariantCulture, "trailer<< /Size {0} /Root 1 0 R >>\nstartxref\n{1}\n%%EOF", objects.Count + 1, xrefStart);
        return Encoding.ASCII.GetBytes(output.ToString());
    }

    private static string SanitizePdfText(string value)
    {
        var ascii = new string(value.Select(ch => ch <= 127 ? ch : '?').ToArray());
        return ascii.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
    }
}
