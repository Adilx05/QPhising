using QPhising.Domain.Tracking.Enums;

namespace QPhising.Domain.Tracking.Models;

public sealed class PageSettings
{
    public const int MinRetentionDays = 1;
    public const int MaxRetentionDays = 3650;

    public PageSettings(int retentionDays, bool captureIpAddress, IpAddressHashPolicy ipAddressHashPolicy, bool enableBotFiltering, bool captureUtmParameters)
    {
        if (retentionDays is < MinRetentionDays or > MaxRetentionDays)
        {
            throw new ArgumentOutOfRangeException(nameof(retentionDays), $"Retention days must be between {MinRetentionDays} and {MaxRetentionDays}.");
        }

        if (!captureIpAddress && ipAddressHashPolicy != IpAddressHashPolicy.None)
        {
            throw new ArgumentException("IP policy must be None when IP capture is disabled.", nameof(ipAddressHashPolicy));
        }

        if (captureIpAddress && ipAddressHashPolicy == IpAddressHashPolicy.None)
        {
            throw new ArgumentException("IP policy must be PlainText or Sha256 when IP capture is enabled.", nameof(ipAddressHashPolicy));
        }

        RetentionDays = retentionDays;
        CaptureIpAddress = captureIpAddress;
        IpAddressHashPolicy = ipAddressHashPolicy;
        EnableBotFiltering = enableBotFiltering;
        CaptureUtmParameters = captureUtmParameters;
    }

    public int RetentionDays { get; }

    public bool CaptureIpAddress { get; }

    public IpAddressHashPolicy IpAddressHashPolicy { get; }

    public bool EnableBotFiltering { get; }

    public bool CaptureUtmParameters { get; }
}
