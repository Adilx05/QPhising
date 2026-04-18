using Microsoft.AspNetCore.DataProtection;
using QPhising.Application.Contracts.Abstractions.RuntimeConfiguration;

namespace QPhising.Api.Services.RuntimeConfiguration;

public sealed class DataProtectionRuntimeConfigurationSecretCipher : IRuntimeConfigurationSecretCipher
{
    private readonly IDataProtector _protector;

    public DataProtectionRuntimeConfigurationSecretCipher(IDataProtectionProvider dataProtectionProvider)
    {
        ArgumentNullException.ThrowIfNull(dataProtectionProvider);
        _protector = dataProtectionProvider.CreateProtector("QPhising.RuntimeConfiguration.SecretCipher.v1");
    }

    public Task<string> EncryptAsync(string plainText, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(plainText))
        {
            throw new ArgumentException("Runtime configuration secret values must not be empty.", nameof(plainText));
        }

        return Task.FromResult(_protector.Protect(plainText));
    }
}
