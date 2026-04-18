using System;

namespace QPhising.Domain.RuntimeConfiguration.ValueObjects;

public sealed record RuntimeSecretValue
{
    public string CipherText { get; }

    public RuntimeSecretValue(string cipherText, string? parameterName = null)
    {
        if (string.IsNullOrWhiteSpace(cipherText))
        {
            throw new ArgumentException("Runtime secret values must not be empty.", parameterName ?? nameof(cipherText));
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

    public static implicit operator string(RuntimeSecretValue value) => value.CipherText;
}
