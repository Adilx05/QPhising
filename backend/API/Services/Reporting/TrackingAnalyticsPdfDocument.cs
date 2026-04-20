using System.Globalization;
using QPhising.Application.Contracts.Abstractions.Reporting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace QPhising.Api.Services.Reporting;

public sealed class TrackingAnalyticsPdfDocument : IDocument
{
    private readonly TrackingReportData _data;
    private readonly string _language;
    private readonly byte[]? _logoBytes;
    private readonly TrackingReportBrandingOptions _branding;
    private readonly CultureInfo _culture;
    private readonly DateTimeOffset _generatedAtUtc = DateTimeOffset.UtcNow;

    public TrackingAnalyticsPdfDocument(
        TrackingReportData data,
        string language,
        byte[]? logoBytes,
        TrackingReportBrandingOptions branding)
    {
        _data = data;
        _language = language;
        _logoBytes = logoBytes;
        _branding = branding;
        _culture = language == "tr" ? new CultureInfo("tr-TR") : new CultureInfo("en-US");
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(24);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(10).FontColor(_branding.TextColor));

            page.Header().Element(ComposeHeader);
            page.Content().PaddingTop(10).Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Row(inner =>
            {
                if (_logoBytes is not null)
                {
                    inner.ConstantItem(120).Height(34).Image(_logoBytes);
                    inner.ConstantItem(12);
                }

                inner.RelativeItem().Column(col =>
                {
                    col.Item().Text(T("Rapor Merkezi", "Report Center"))
                        .FontSize(18)
                        .SemiBold()
                        .FontColor("#1F2937");

                    col.Item().Text(T("Takip analiz dışa aktarımı", "Tracking analytics export"))
                        .FontSize(9)
                        .FontColor(_branding.MutedTextColor);
                });
            });

            row.ConstantItem(220).AlignRight().Column(col =>
            {
                col.Item().AlignRight().Text($"{T("Oluşturulma", "Generated")}: {FormatDate(_generatedAtUtc)}")
                    .FontSize(9)
                    .FontColor(_branding.MutedTextColor);

                col.Item().AlignRight().Text($"{T("Dil", "Language")}: {(_language == "tr" ? "Türkçe" : "English")}")
                    .FontSize(9)
                    .FontColor(_branding.MutedTextColor);
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.Column(col =>
        {
            col.Spacing(14);

            // Page 1
            col.Item().Element(ComposeSummaryBanner);
            col.Item().Element(ComposeKpiCards);

            if (_data.AppliedNotes.Count > 0)
                col.Item().Element(ComposeNotesSection);

            // Page Break
            col.Item().PageBreak();

            // Page 2
            col.Item().Element(ComposeTrendSection);
            col.Item().Element(ComposeDistributionSection);

            if (_data.VisitorRows.Count > 0)
            {
                col.Item().PageBreak();
                col.Item().Element(ComposeVisitorsSection);
            }
        });
    }

    private void ComposeSummaryBanner(IContainer container)
    {
        container.Element(Card).Column(col =>
        {
            col.Spacing(10);

            col.Item().Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    left.Spacing(4);

                    left.Item().Text(_data.ScopeLabel)
                        .FontSize(20)
                        .SemiBold()
                        .FontColor("#0F172A");

                    left.Item().Text(T("Kapsam", "Scope"))
                        .FontSize(9)
                        .FontColor(_branding.MutedTextColor);
                });

                row.ConstantItem(240).Column(right =>
                {
                    right.Spacing(4);

                    right.Item().Text(T("Tarih Aralığı", "Date Range"))
                        .FontSize(10)
                        .SemiBold()
                        .FontColor("#0F172A");

                    right.Item().Text($"{FormatNullableDate(_data.FromUtc)} - {FormatNullableDate(_data.ToUtc, true)}")
                        .FontSize(10)
                        .FontColor(_branding.TextColor);
                });
            });
        });
    }

    private void ComposeKpiCards(IContainer container)
    {
        var engagement = _data.TotalVisits == 0
            ? 0m
            : Math.Round((_data.UniqueVisitors / (decimal)_data.TotalVisits) * 100m, 1);

        container.Row(row =>
        {
            row.Spacing(10);

            row.RelativeItem().Element(c => ComposeKpiCard(c, T("Toplam Ziyaret", "Total Visits"), _data.TotalVisits.ToString("N0", _culture)));
            row.RelativeItem().Element(c => ComposeKpiCard(c, T("Benzersiz Ziyaretçi", "Unique Visitors"), _data.UniqueVisitors.ToString("N0", _culture)));
            row.RelativeItem().Element(c => ComposeKpiCard(c, T("Etkileşim Oranı", "Engagement Rate"), $"{engagement.ToString("N1", _culture)}%"));
            row.RelativeItem().Element(c => ComposeKpiCard(c, T("Son Ziyaret", "Last Visit"), _data.LastVisitAtUtc is null ? "-" : FormatDate(_data.LastVisitAtUtc.Value)));
        });
    }

    private void ComposeKpiCard(IContainer container, string title, string value)
    {
        container.Element(Card).Column(col =>
        {
            col.Spacing(6);
            col.Item().Text(title).FontSize(10).FontColor(_branding.MutedTextColor);
            col.Item().Text(value).FontSize(18).SemiBold().FontColor("#0F172A");
        });
    }

    private void ComposeTrendSection(IContainer container)
    {
        var trendImage = TrackingChartRenderer.RenderTrendChart(
            _data.TrendRows,
            _language,
            _branding.PrimaryColor,
            _branding.AccentColor,
            _branding.BorderColor,
            _branding.TextColor);

        container
            .ShowEntire()
            .Element(Card)
            .Column(col =>
            {
                col.Spacing(8);

                col.Item().Text(T("Ziyaret Trendi", "Visit Trend"))
                    .FontSize(14)
                    .SemiBold()
                    .FontColor("#0F172A");

                col.Item().Text(T(
                    "Toplam ziyaret ve benzersiz ziyaretçi eğilimi zaman içinde gösterilir.",
                    "Shows total visits and unique visitor movement over time."))
                    .FontSize(9)
                    .FontColor(_branding.MutedTextColor);

                col.Item().Row(row =>
                {
                    row.Spacing(16);

                    row.AutoItem().Row(r =>
                    {
                        r.Spacing(6);
                        r.AutoItem().AlignMiddle().Height(4).Width(18).Background(_branding.PrimaryColor);
                        r.AutoItem().AlignMiddle().Text(T("Toplam Ziyaret", "Total Visits"))
                            .FontSize(9)
                            .FontColor(_branding.MutedTextColor);
                    });

                    row.AutoItem().Row(r =>
                    {
                        r.Spacing(6);
                        r.AutoItem().AlignMiddle().Height(4).Width(18).Background(_branding.AccentColor);
                        r.AutoItem().AlignMiddle().Text(T("Benzersiz Ziyaretçi", "Unique Visitors"))
                            .FontSize(9)
                            .FontColor(_branding.MutedTextColor);
                    });
                });

                col.Item().PaddingTop(6).MaxHeight(260).Image(trendImage).FitArea();
            });
    }

    private void ComposeDistributionSection(IContainer container)
    {
        var distributionImage = TrackingChartRenderer.RenderDistributionChart(
            _data.DistributionRows,
            _language,
            _branding.PrimaryColor,
            _branding.AccentColor,
            _branding.BorderColor,
            _branding.TextColor);

        container.Element(Card).Column(col =>
        {
            col.Spacing(12);

            col.Item().Text(T("Dağılım Özeti", "Distribution Overview"))
                .FontSize(14)
                .SemiBold()
                .FontColor("#0F172A");

            col.Item().Text(T(
                    "En yüksek hacimli kırılımlar yatay çubuk grafikte gösterilir.",
                    "Highest-volume breakdowns are displayed in a ranked horizontal bar chart."))
                .FontSize(9)
                .FontColor(_branding.MutedTextColor);

            col.Item().MaxHeight(220).Image(distributionImage).FitArea();

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(1);
                });

                table.Header(header =>
                {
                    header.Cell().Element(TableHeaderCell).Text(T("Etiket", "Label"));
                    header.Cell().Element(TableHeaderCell).AlignRight().Text(T("Adet", "Count"));
                });

                foreach (var row in _data.DistributionRows.Take(10))
                {
                    table.Cell().Element(TableCell).Text(Shorten(row.Label, 80));
                    table.Cell().Element(TableCell).AlignRight().Text(row.Value.ToString("N0", _culture));
                }
            });
        });
    }

    private void ComposeNotesSection(IContainer container)
    {
        container.Element(Card).Column(col =>
        {
            col.Spacing(8);

            col.Item().Text(T("Uygulanan Notlar", "Applied Notes"))
                .FontSize(14)
                .SemiBold()
                .FontColor("#0F172A");

            foreach (var note in _data.AppliedNotes)
            {
                col.Item().Row(row =>
                {
                    row.ConstantItem(10).Text("•").FontColor(_branding.PrimaryColor);
                    row.RelativeItem().Text(note).FontColor(_branding.TextColor);
                });
            }
        });
    }

    private void ComposeVisitorsSection(IContainer container)
    {
        container.Element(Card).Column(col =>
        {
            col.Spacing(10);

            col.Item().Text(T("Ziyaretçi Detayları", "Visitor Details"))
                .FontSize(14)
                .SemiBold()
                .FontColor("#0F172A");

            col.Item().Text(T(
                    "Detaylı mod aktif olduğunda ziyaretçi bazlı kırılım eklenir.",
                    "Visitor-level breakdown is included when detailed mode is enabled."))
                .FontSize(9)
                .FontColor(_branding.MutedTextColor);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(0.8f);
                    columns.RelativeColumn(1.5f);
                });

                table.Header(header =>
                {
                    header.Cell().Element(TableHeaderCell).Text(T("Ziyaretçi", "Visitor"));
                    header.Cell().Element(TableHeaderCell).Text(T("Oturum", "Session"));
                    header.Cell().Element(TableHeaderCell).Text(T("Fingerprint", "Fingerprint"));
                    header.Cell().Element(TableHeaderCell).Text(T("IP Hash", "IP Hash"));
                    header.Cell().Element(TableHeaderCell).AlignRight().Text(T("Tık", "Clicks"));
                    header.Cell().Element(TableHeaderCell).Text(T("Son Görülme", "Last Seen"));
                });

                foreach (var row in _data.VisitorRows)
                {
                    table.Cell().Element(TableCell).Text(Shorten(row.VisitorKey, 36));
                    table.Cell().Element(TableCell).Text(Shorten(row.SessionId ?? "-", 24));
                    table.Cell().Element(TableCell).Text(Shorten(row.VisitorFingerprint ?? "-", 24));
                    table.Cell().Element(TableCell).Text(Shorten(row.IpHash ?? "-", 24));
                    table.Cell().Element(TableCell).AlignRight().Text(row.ClickCount.ToString("N0", _culture));
                    table.Cell().Element(TableCell).Text(FormatDate(row.LastOccurredAtUtc));
                }
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.PaddingTop(8).BorderTop(1).BorderColor(_branding.BorderColor).Row(row =>
        {
            row.RelativeItem().Text($"{T("Dışa Aktarım", "Export")}: {FormatDate(_generatedAtUtc)}")
                .FontSize(8)
                .FontColor(_branding.MutedTextColor);

            row.ConstantItem(140).AlignRight().Text(text =>
            {
                text.Span(T("Sayfa ", "Page ")).FontSize(8).FontColor(_branding.MutedTextColor);
                text.CurrentPageNumber().FontSize(8).FontColor(_branding.MutedTextColor);
                text.Span(" / ").FontSize(8).FontColor(_branding.MutedTextColor);
                text.TotalPages().FontSize(8).FontColor(_branding.MutedTextColor);
            });
        });
    }

    private IContainer Card(IContainer container) =>
        container
            .Background(_branding.CardColor)
            .Border(1)
            .BorderColor(_branding.BorderColor)
            .CornerRadius(12)
            .Padding(16);

    private IContainer TableHeaderCell(IContainer container) =>
        container
            .Background(_branding.SurfaceColor)
            .BorderBottom(1)
            .BorderColor(_branding.BorderColor)
            .PaddingVertical(8)
            .PaddingHorizontal(8);

    private IContainer TableCell(IContainer container) =>
        container
            .BorderBottom(1)
            .BorderColor("#F1F5F9")
            .PaddingVertical(7)
            .PaddingHorizontal(8);

    private string T(string tr, string en) => _language == "tr" ? tr : en;

    private string FormatNullableDate(DateTimeOffset? value, bool isEnd = false)
    {
        if (value is null)
            return T("Tüm Zamanlar", "All Time");

        return FormatDate(value.Value);
    }

    private string FormatDate(DateTimeOffset value)
    {
        return _language == "tr"
            ? value.ToString("dd.MM.yyyy HH:mm", _culture)
            : value.ToString("yyyy-MM-dd HH:mm", _culture);
    }

    private static string Shorten(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "-";

        return value.Length <= maxLength
            ? value
            : value[..(maxLength - 1)] + "…";
    }
}