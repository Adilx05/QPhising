using MediatR;
using QPhising.Application.Contracts.Abstractions.Setup;
using QPhising.Application.Contracts.Responses.Setup;

namespace QPhising.Application.CQRS.Queries.Setup;

public sealed class GetSetupGuardDecisionQueryHandler : IRequestHandler<GetSetupGuardDecisionQuery, SetupGuardDecisionResult>
{
    private readonly ISetupConfigurationRepository _setupConfigurationRepository;
    private readonly ISetupBootstrapConfigurationReader _setupBootstrapConfigurationReader;

    public GetSetupGuardDecisionQueryHandler(
        ISetupConfigurationRepository setupConfigurationRepository,
        ISetupBootstrapConfigurationReader setupBootstrapConfigurationReader)
    {
        _setupConfigurationRepository = setupConfigurationRepository;
        _setupBootstrapConfigurationReader = setupBootstrapConfigurationReader;
    }

    public async Task<SetupGuardDecisionResult> Handle(
        GetSetupGuardDecisionQuery request,
        CancellationToken cancellationToken)
    {
        var aggregate = await _setupConfigurationRepository.GetCurrentAsync(cancellationToken);
        if (aggregate is not null)
        {
            return SetupGuardDecisionResult.FromAggregate(aggregate);
        }

        return _setupBootstrapConfigurationReader.IsBootstrapConfigurationReady()
            ? SetupGuardDecisionResult.MainApplicationAccessibleWithoutWizard()
            : SetupGuardDecisionResult.SetupRequired();
    }
}
