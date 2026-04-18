using MediatR;
using QPhising.Application.Contracts.Abstractions.RuntimeConfiguration;
using QPhising.Application.Contracts.Responses.RuntimeConfiguration;
using QPhising.Domain.RuntimeConfiguration.Aggregates;

namespace QPhising.Application.CQRS.Commands.RuntimeConfiguration;

public sealed class SaveRuntimeConfigurationCommandHandler : IRequestHandler<SaveRuntimeConfigurationCommand, RuntimeConfigurationResult>
{
    private readonly IRuntimeConfigurationRepository _runtimeConfigurationRepository;
    private readonly IRuntimeConfigurationSecretCipher _runtimeConfigurationSecretCipher;

    public SaveRuntimeConfigurationCommandHandler(
        IRuntimeConfigurationRepository runtimeConfigurationRepository,
        IRuntimeConfigurationSecretCipher runtimeConfigurationSecretCipher)
    {
        _runtimeConfigurationRepository = runtimeConfigurationRepository;
        _runtimeConfigurationSecretCipher = runtimeConfigurationSecretCipher;
    }

    public async Task<RuntimeConfigurationResult> Handle(SaveRuntimeConfigurationCommand request, CancellationToken cancellationToken)
    {
        var aggregate = new RuntimeConfigurationAggregate();

        var databaseCipherText = await _runtimeConfigurationSecretCipher.EncryptAsync(request.DatabaseConnectionString, cancellationToken);
        var redisCipherText = await _runtimeConfigurationSecretCipher.EncryptAsync(request.RedisConnectionString, cancellationToken);
        var keycloakClientSecretCipherText = await _runtimeConfigurationSecretCipher.EncryptAsync(request.KeycloakClientSecret, cancellationToken);

        aggregate.SetDatabaseConnection(databaseCipherText);
        aggregate.SetRedisConnection(redisCipherText);
        aggregate.SetKeycloakConfiguration(
            new Uri(request.KeycloakAuthority),
            request.KeycloakRealm,
            request.KeycloakClientId,
            keycloakClientSecretCipherText);

        await _runtimeConfigurationRepository.SaveAsync(aggregate, cancellationToken);

        return RuntimeConfigurationResult.FromAggregate(aggregate);
    }
}
