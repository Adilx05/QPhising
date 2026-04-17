using Microsoft.AspNetCore.DataProtection;
using QPhising.Application.Contracts.Abstractions.Setup;

namespace QPhising.Api.Services.Setup;

public sealed class DataProtectionSetupSecretCipher : ISetupSecretCipher
{
    private readonly IDataProtector _protector;

    public DataProtectionSetupSecretCipher(IDataProtectionProvider dataProtectionProvider)
    {
        ArgumentNullException.ThrowIfNull(dataProtectionProvider);
        _protector = dataProtectionProvider.CreateProtector("QPhising.Setup.SecretCipher.v1");
    }

    public Task<string> EncryptAsync(string plainText, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(plainText))
        {
            throw new ArgumentException("Setup secret values must not be empty.", nameof(plainText));
        }

        return Task.FromResult(_protector.Protect(plainText));
    }
}
