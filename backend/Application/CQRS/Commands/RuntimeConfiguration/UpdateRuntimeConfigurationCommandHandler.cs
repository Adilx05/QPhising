using MediatR;
using QPhising.Application.Contracts.Abstractions.RuntimeConfiguration;
using QPhising.Application.Contracts.Responses.RuntimeConfiguration;
using QPhising.Domain.RuntimeConfiguration.Aggregates;

namespace QPhising.Application.CQRS.Commands.RuntimeConfiguration;

public sealed class UpdateRuntimeConfigurationCommandHandler : IRequestHandler<UpdateRuntimeConfigurationCommand, RuntimeConfigurationResult>
{
    private readonly IRuntimeConfigurationRepository _runtimeConfigurationRepository;
    private readonly IRuntimeConfigurationSecretCipher _runtimeConfigurationSecretCipher;

    public UpdateRuntimeConfigurationCommandHandler(
        IRuntimeConfigurationRepository runtimeConfigurationRepository,
        IRuntimeConfigurationSecretCipher runtimeConfigurationSecretCipher)
    {
        _runtimeConfigurationRepository = runtimeConfigurationRepository;
        _runtimeConfigurationSecretCipher = runtimeConfigurationSecretCipher;
    }

    public async Task<RuntimeConfigurationResult> Handle(UpdateRuntimeConfigurationCommand request, CancellationToken cancellationToken)
    {
        var aggregate = await _runtimeConfigurationRepository.GetCurrentAsync(cancellationToken)
            ?? new RuntimeConfigurationAggregate();

        if (!string.IsNullOrWhiteSpace(request.DatabaseConnectionString))
        {
            var databaseCipherText = await _runtimeConfigurationSecretCipher.EncryptAsync(request.DatabaseConnectionString, cancellationToken);
            aggregate.SetDatabaseConnection(databaseCipherText);
        }

        if (!string.IsNullOrWhiteSpace(request.RedisConnectionString))
        {
            var redisCipherText = await _runtimeConfigurationSecretCipher.EncryptAsync(request.RedisConnectionString, cancellationToken);
            aggregate.SetRedisConnection(redisCipherText);
        }

        if (!string.IsNullOrWhiteSpace(request.KeycloakAuthority))
        {
            var keycloakSecretCipherText = await _runtimeConfigurationSecretCipher.EncryptAsync(request.KeycloakClientSecret!, cancellationToken);
            aggregate.SetKeycloakConfiguration(
                new Uri(request.KeycloakAuthority),
                request.KeycloakRealm!,
                request.KeycloakClientId!,
                keycloakSecretCipherText);
        }

        await _runtimeConfigurationRepository.SaveAsync(aggregate, cancellationToken);

        return RuntimeConfigurationResult.FromAggregate(aggregate);
    }
}
