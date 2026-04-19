namespace QPhising.Application.Contracts.Responses.Tracking;

public sealed record TrackingPageSettingsResult(
    int RetentionDays,
    bool MaskIpAddress,
    bool EnableBotFiltering,
    bool CaptureUtmParameters);
