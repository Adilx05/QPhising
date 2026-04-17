using System;

namespace QPhising.Domain.Setup.ValueObjects;

public sealed record SecureConfigValue
{
    public string CipherText { get; }

    public SecureConfigValue(string cipherText)
    {
        if (string.IsNullOrWhiteSpace(cipherText))
        {
            throw new ArgumentException("Secure configuration values must not be empty.", nameof(cipherText));
        }

        CipherText = cipherText.Trim();
    }

    public string ToMaskedPreview(int visibleSuffixLength = 4)
    {
        if (visibleSuffixLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(visibleSuffixLength), "Visible suffix length must be non-negative.");
        }

        if (CipherText.Length <= visibleSuffixLength)
        {
            return new string('*', CipherText.Length);
        }

        var suffix = CipherText[^visibleSuffixLength..];
        return $"{new string('*', CipherText.Length - visibleSuffixLength)}{suffix}";
    }

    public static implicit operator string(SecureConfigValue value) => value.CipherText;
}
