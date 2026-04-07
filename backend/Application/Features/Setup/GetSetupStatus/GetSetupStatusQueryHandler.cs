using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Setup;

namespace QPhising.Application.Features.Setup.GetSetupStatus;

public sealed class GetSetupStatusQueryHandler(ISetupStateRepository setupStateRepository)
    : IRequestHandler<GetSetupStatusQuery, Result<SetupStatusResponse>>
{
    public async Task<Result<SetupStatusResponse>> Handle(GetSetupStatusQuery request, CancellationToken cancellationToken)
    {
        SetupState? setupState = await setupStateRepository.GetAsync(cancellationToken);

        SetupStatusResponse response = setupState is null
            ? new SetupStatusResponse(false, null)
            : new SetupStatusResponse(setupState.IsCompleted, setupState.CompletedAtUtc);

        return Result<SetupStatusResponse>.Success(response);
    }
}
