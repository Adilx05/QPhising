namespace QPhising.Application.Contracts.Abstractions.RuntimeConfiguration;

public interface IRuntimeConfigurationSecretCipher
{
    Task<string> EncryptAsync(string plaintext, CancellationToken cancellationToken);
}
