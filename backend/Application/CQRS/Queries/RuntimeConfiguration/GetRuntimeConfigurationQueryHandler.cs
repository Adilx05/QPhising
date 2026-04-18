using MediatR;
using QPhising.Application.Contracts.Abstractions.RuntimeConfiguration;
using QPhising.Application.Contracts.Responses.RuntimeConfiguration;

namespace QPhising.Application.CQRS.Queries.RuntimeConfiguration;

public sealed class GetRuntimeConfigurationQueryHandler : IRequestHandler<GetRuntimeConfigurationQuery, RuntimeConfigurationResult>
{
    private readonly IRuntimeConfigurationRepository _runtimeConfigurationRepository;

    public GetRuntimeConfigurationQueryHandler(IRuntimeConfigurationRepository runtimeConfigurationRepository)
    {
        _runtimeConfigurationRepository = runtimeConfigurationRepository;
    }

    public async Task<RuntimeConfigurationResult> Handle(GetRuntimeConfigurationQuery request, CancellationToken cancellationToken)
    {
        var aggregate = await _runtimeConfigurationRepository.GetCurrentAsync(cancellationToken);
        return aggregate is null ? RuntimeConfigurationResult.Empty() : RuntimeConfigurationResult.FromAggregate(aggregate);
    }
}
