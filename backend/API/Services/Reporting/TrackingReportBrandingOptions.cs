namespace QPhising.Api.Services.Reporting;

public sealed class TrackingReportBrandingOptions
{
    public string LogoPath { get; set; } = "Assets/Branding/logo.png";
    public string PrimaryColor { get; set; } = "#2563EB";
    public string AccentColor { get; set; } = "#10B981";
    public string TextColor { get; set; } = "#334155";
    public string MutedTextColor { get; set; } = "#64748B";
    public string BorderColor { get; set; } = "#E2E8F0";
    public string SurfaceColor { get; set; } = "#F8FAFC";
    public string CardColor { get; set; } = "#FFFFFF";
}