using QPhising.Domain.Tracking.Enums;

namespace QPhising.Application.Contracts.Responses.Tracking;

public sealed record TrackingPageSettingsResult(
    int RetentionDays,
    bool CaptureIpAddress,
    IpAddressHashPolicy IpAddressHashPolicy,
    bool EnableBotFiltering,
    bool CaptureUtmParameters);
