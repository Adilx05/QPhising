using QPhising.Application.Contracts.Abstractions.Reporting;
using QuestPDF.Helpers;
using SkiaSharp;
using System.Globalization;

namespace QPhising.Api.Services.Reporting;

public static class TrackingChartRenderer
{
    public static byte[] RenderTrendChart(
        IReadOnlyCollection<(string Label, int TotalVisits, int UniqueVisitors)> rows,
        string language,
        string primaryColorHex,
        string accentColorHex,
        string borderColorHex,
        string textColorHex)
    {
        const int width = 1200;
        const int height = 520;
        const int left = 80;
        const int right = 30;
        const int top = 40;
        const int bottom = 70;

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        var plotWidth = width - left - right;
        var plotHeight = height - top - bottom;

        var items = rows.Take(16).ToArray();
        if (items.Length == 0)
            return Encode(bitmap);

        var maxY = Math.Max(1, items.Max(x => Math.Max(x.TotalVisits, x.UniqueVisitors)));

        using var gridPaint = new SKPaint
        {
            Color = SKColor.Parse(borderColorHex),
            StrokeWidth = 1,
            IsAntialias = true
        };

        using var axisPaint = new SKPaint
        {
            Color = SKColor.Parse(textColorHex),
            StrokeWidth = 1.5f,
            IsAntialias = true
        };

        using var totalPaint = new SKPaint
        {
            Color = SKColor.Parse(primaryColorHex),
            StrokeWidth = 3,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        using var uniquePaint = new SKPaint
        {
            Color = SKColor.Parse(accentColorHex),
            StrokeWidth = 3,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        using var pointTotalPaint = new SKPaint
        {
            Color = SKColor.Parse(primaryColorHex),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        using var pointUniquePaint = new SKPaint
        {
            Color = SKColor.Parse(accentColorHex),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        using var labelPaint = new SKPaint
        {
            Color = SKColor.Parse(textColorHex),
            IsAntialias = true
        };

        for (var i = 0; i <= 4; i++)
        {
            var y = top + (plotHeight / 4f) * i;
            canvas.DrawLine(left, y, width - right, y, gridPaint);
        }

        canvas.DrawLine(left, top, left, height - bottom, axisPaint);
        canvas.DrawLine(left, height - bottom, width - right, height - bottom, axisPaint);

        var totalPath = new SKPath();
        var uniquePath = new SKPath();

        for (var i = 0; i < items.Length; i++)
        {
            var x = left + (plotWidth * (i / (float)Math.Max(1, items.Length - 1)));
            var totalY = top + plotHeight - ((items[i].TotalVisits / (float)maxY) * plotHeight);
            var uniqueY = top + plotHeight - ((items[i].UniqueVisitors / (float)maxY) * plotHeight);

            if (i == 0)
            {
                totalPath.MoveTo(x, totalY);
                uniquePath.MoveTo(x, uniqueY);
            }
            else
            {
                totalPath.LineTo(x, totalY);
                uniquePath.LineTo(x, uniqueY);
            }

            canvas.DrawCircle(x, totalY, 4, pointTotalPaint);
            canvas.DrawCircle(x, uniqueY, 4, pointUniquePaint);

        }

        canvas.DrawPath(totalPath, totalPaint);
        canvas.DrawPath(uniquePath, uniquePaint);

        using var totalLegendPaint = new SKPaint { Color = SKColor.Parse(primaryColorHex), StrokeWidth = 6, IsAntialias = true };
        canvas.DrawLine(width - 380, 18, width - 330, 18, totalLegendPaint);

        using var uniqueLegendPaint = new SKPaint { Color = SKColor.Parse(accentColorHex), StrokeWidth = 6, IsAntialias = true };
        canvas.DrawLine(width - 380, 45, width - 330, 45, uniqueLegendPaint);

        return Encode(bitmap);
    }

    public static byte[] RenderDistributionChart(
        IReadOnlyCollection<(string Label, int Value)> rows,
        string language,
        string primaryColorHex,
        string accentColorHex,
        string borderColorHex,
        string textColorHex)
    {
        const int width = 1200;
        const int height = 520;
        const int left = 300;
        const int right = 60;
        const int top = 40;
        const int bottom = 40;

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        var items = rows.Take(8).ToArray();
        if (items.Length == 0)
            return Encode(bitmap);

        var max = Math.Max(1, items.Max(x => x.Value));
        var areaHeight = height - top - bottom;
        var rowHeight = areaHeight / items.Length;

        using var labelPaint = new SKPaint
        {
            Color = SKColor.Parse(textColorHex),
            IsAntialias = true
        };

        using var valuePaint = new SKPaint
        {
            Color = SKColor.Parse(textColorHex),
            IsAntialias = true,
        };

        using var railPaint = new SKPaint
        {
            Color = SKColor.Parse(borderColorHex),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        var palette = new[]
        {
            SKColor.Parse(primaryColorHex),
            SKColor.Parse(accentColorHex),
            SKColor.Parse("#60A5FA"),
            SKColor.Parse("#34D399"),
            SKColor.Parse("#93C5FD"),
            SKColor.Parse("#6EE7B7"),
            SKColor.Parse("#BFDBFE"),
            SKColor.Parse("#A7F3D0")
        };

        for (var i = 0; i < items.Length; i++)
        {
            var y = top + i * rowHeight + 8;
            var barY = y + 18;
            var barHeight = 20;
            var availableWidth = width - left - right;
            var actualWidth = (items[i].Value / (float)max) * availableWidth;

            canvas.DrawRoundRect(new SKRoundRect(new SKRect(left, barY, width - right, barY + barHeight), 6, 6), railPaint);

            using var fillPaint = new SKPaint
            {
                Color = palette[i % palette.Length],
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            canvas.DrawRoundRect(new SKRoundRect(new SKRect(left, barY, left + actualWidth, barY + barHeight), 6, 6), fillPaint);
        }

        return Encode(bitmap);
    }

    private static byte[] Encode(SKBitmap bitmap)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
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