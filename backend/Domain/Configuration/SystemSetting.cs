namespace QPhising.Domain.Configuration;

public sealed class SystemSetting
{
    private SystemSetting(string key, string value, DateTimeOffset updatedAtUtc)
    {
        Key = key;
        Value = value;
        UpdatedAtUtc = updatedAtUtc;
    }

    private SystemSetting()
    {
    }

    public string Key { get; private set; } = string.Empty;

    public string Value { get; private set; } = string.Empty;

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static SystemSetting Create(string key, string value, DateTimeOffset updatedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("System setting key is required.", nameof(key));
        }

        return new SystemSetting(key.Trim(), value.Trim(), updatedAtUtc);
    }

    public void SetValue(string value, DateTimeOffset updatedAtUtc)
    {
        Value = value.Trim();
        UpdatedAtUtc = updatedAtUtc;
    }
}
