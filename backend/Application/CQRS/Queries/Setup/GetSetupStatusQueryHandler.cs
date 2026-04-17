using MediatR;
using QPhising.Application.Contracts.Abstractions.Setup;
using QPhising.Application.Contracts.Responses.Setup;

namespace QPhising.Application.CQRS.Queries.Setup;

public sealed class GetSetupStatusQueryHandler : IRequestHandler<GetSetupStatusQuery, SetupStatusResult>
{
    private readonly ISetupConfigurationRepository _setupConfigurationRepository;

    public GetSetupStatusQueryHandler(ISetupConfigurationRepository setupConfigurationRepository)
    {
        _setupConfigurationRepository = setupConfigurationRepository;
    }

    public async Task<SetupStatusResult> Handle(GetSetupStatusQuery request, CancellationToken cancellationToken)
    {
        var aggregate = await _setupConfigurationRepository.GetCurrentAsync(cancellationToken);
        return aggregate is null ? SetupStatusResult.NotStarted() : SetupStatusResult.FromAggregate(aggregate);
    }
}
