using MediatR;
using QPhising.Application.Contracts.Abstractions.Setup;
using QPhising.Application.Contracts.Responses.Setup;
using QPhising.Domain.Setup.Aggregates;
using QPhising.Domain.Setup.ValueObjects;

namespace QPhising.Application.CQRS.Commands.Setup;

public sealed class SaveSetupConfigurationCommandHandler : IRequestHandler<SaveSetupConfigurationCommand, SetupStatusResult>
{
    private readonly ISetupConfigurationRepository _setupConfigurationRepository;
    private readonly ISetupSecretCipher _setupSecretCipher;

    public SaveSetupConfigurationCommandHandler(
        ISetupConfigurationRepository setupConfigurationRepository,
        ISetupSecretCipher setupSecretCipher)
    {
        _setupConfigurationRepository = setupConfigurationRepository;
        _setupSecretCipher = setupSecretCipher;
    }

    public async Task<SetupStatusResult> Handle(SaveSetupConfigurationCommand request, CancellationToken cancellationToken)
    {
        var aggregate = new SetupAggregate();

        var databaseCipherText = await _setupSecretCipher.EncryptAsync(request.DatabaseConnectionString, cancellationToken);
        var redisCipherText = await _setupSecretCipher.EncryptAsync(request.RedisConnectionString, cancellationToken);
        var keycloakSecretCipherText = await _setupSecretCipher.EncryptAsync(request.KeycloakClientSecret, cancellationToken);

        aggregate.ConfigureDatabase(new SecureConfigValue(databaseCipherText));
        aggregate.ConfigureRedis(new SecureConfigValue(redisCipherText));
        aggregate.ConfigureKeycloak(
            new Uri(request.KeycloakAuthority),
            request.KeycloakRealm,
            request.KeycloakClientId,
            new SecureConfigValue(keycloakSecretCipherText));
        aggregate.MarkSetupCompleted();

        await _setupConfigurationRepository.SaveAsync(aggregate, cancellationToken);

        return SetupStatusResult.FromAggregate(aggregate);
    }
}
