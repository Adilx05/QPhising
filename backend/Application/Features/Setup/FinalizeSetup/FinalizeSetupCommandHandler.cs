using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Setup;

namespace QPhising.Application.Features.Setup.FinalizeSetup;

public sealed class FinalizeSetupCommandHandler(
    ISetupStateRepository setupStateRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<FinalizeSetupCommand, Result<FinalizeSetupResponse>>
{
    public async Task<Result<FinalizeSetupResponse>> Handle(FinalizeSetupCommand request, CancellationToken cancellationToken)
    {
        SetupState? setupState = await setupStateRepository.GetAsync(cancellationToken);
        DateTimeOffset now = DateTimeOffset.UtcNow;

        if (setupState is null)
        {
            setupState = SetupState.CreateInitial();
            setupState.Finalize(now);
            await setupStateRepository.AddAsync(setupState, cancellationToken);
        }
        else if (!setupState.IsCompleted)
        {
            setupState.Finalize(now);
            setupStateRepository.Update(setupState);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        DateTimeOffset completedAtUtc = setupState.CompletedAtUtc ?? now;
        return Result<FinalizeSetupResponse>.Success(new FinalizeSetupResponse(true, completedAtUtc));
    }
}
