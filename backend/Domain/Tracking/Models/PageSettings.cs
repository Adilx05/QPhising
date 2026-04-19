namespace QPhising.Domain.Tracking.Models;

public sealed class PageSettings
{
    public const int MinRetentionDays = 1;
    public const int MaxRetentionDays = 3650;

    public PageSettings(int retentionDays, bool maskIpAddress, bool enableBotFiltering, bool captureUtmParameters)
    {
        if (retentionDays is < MinRetentionDays or > MaxRetentionDays)
        {
            throw new ArgumentOutOfRangeException(nameof(retentionDays), $"Retention days must be between {MinRetentionDays} and {MaxRetentionDays}.");
        }

        RetentionDays = retentionDays;
        MaskIpAddress = maskIpAddress;
        EnableBotFiltering = enableBotFiltering;
        CaptureUtmParameters = captureUtmParameters;
    }

    public int RetentionDays { get; }

    public bool MaskIpAddress { get; }

    public bool EnableBotFiltering { get; }

    public bool CaptureUtmParameters { get; }
}
