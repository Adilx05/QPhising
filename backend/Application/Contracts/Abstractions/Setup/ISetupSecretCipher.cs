namespace QPhising.Application.Contracts.Abstractions.Setup;

public interface ISetupSecretCipher
{
    Task<string> EncryptAsync(string plainText, CancellationToken cancellationToken);
}
