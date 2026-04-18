using System;

using QPhising.Domain.RuntimeConfiguration.ValueObjects;

namespace QPhising.Domain.RuntimeConfiguration.Policies;

public static class RuntimeConfigurationSecretPolicy
{
    public static RuntimeSecretValue RequireSecret(string cipherText, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
        {
            throw new ArgumentException("Parameter name is required.", nameof(parameterName));
        }

        return new RuntimeSecretValue(cipherText, parameterName);
    }

    public static bool HasSecretChanged(RuntimeSecretValue? currentValue, RuntimeSecretValue nextValue)
    {
        ArgumentNullException.ThrowIfNull(nextValue);

        return currentValue is null || !string.Equals(currentValue.CipherText, nextValue.CipherText, StringComparison.Ordinal);
    }
}
