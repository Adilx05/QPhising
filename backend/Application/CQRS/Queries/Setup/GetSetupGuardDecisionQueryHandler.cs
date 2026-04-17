using MediatR;
using QPhising.Application.Contracts.Abstractions.Setup;
using QPhising.Application.Contracts.Responses.Setup;

namespace QPhising.Application.CQRS.Queries.Setup;

public sealed class GetSetupGuardDecisionQueryHandler : IRequestHandler<GetSetupGuardDecisionQuery, SetupGuardDecisionResult>
{
    private readonly ISetupConfigurationRepository _setupConfigurationRepository;

    public GetSetupGuardDecisionQueryHandler(ISetupConfigurationRepository setupConfigurationRepository)
    {
        _setupConfigurationRepository = setupConfigurationRepository;
    }

    public async Task<SetupGuardDecisionResult> Handle(
        GetSetupGuardDecisionQuery request,
        CancellationToken cancellationToken)
    {
        var aggregate = await _setupConfigurationRepository.GetCurrentAsync(cancellationToken);
        return aggregate is null
            ? SetupGuardDecisionResult.SetupRequired()
            : SetupGuardDecisionResult.FromAggregate(aggregate);
    }
}
