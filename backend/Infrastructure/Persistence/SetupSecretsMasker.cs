using System.Text.RegularExpressions;

namespace QPhising.Infrastructure.Persistence;

public static partial class SetupSecretsMasker
{
    private const string Mask = "***";

    public static string MaskSecrets(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        string masked = value;
        masked = PasswordConnectionStringRegex().Replace(masked, match => $"{match.Groups["key"].Value}{Mask}");
        masked = UserIdConnectionStringRegex().Replace(masked, match => $"{match.Groups["key"].Value}{Mask}");
        masked = JsonPasswordRegex().Replace(masked, match => $"{match.Groups["prefix"].Value}{Mask}{match.Groups["suffix"].Value}");
        masked = JsonSecretRegex().Replace(masked, match => $"{match.Groups["prefix"].Value}{Mask}{match.Groups["suffix"].Value}");
        return masked;
    }

    [GeneratedRegex("(?<key>(?:Password|Pwd)\\s*=\\s*)[^;\\s]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PasswordConnectionStringRegex();

    [GeneratedRegex("(?<key>User\\s*Id\\s*=\\s*)[^;\\s]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex UserIdConnectionStringRegex();

    [GeneratedRegex("(?<prefix>\"password\"\\s*:\\s*\")[^\"]*(?<suffix>\")", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex JsonPasswordRegex();

    [GeneratedRegex("(?<prefix>\"secret\"\\s*:\\s*\")[^\"]*(?<suffix>\")", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex JsonSecretRegex();
}
