using MediatR;
using QPhising.Application.Contracts.Abstractions.Tracking;

namespace QPhising.Application.CQRS.Commands.Tracking;

public sealed class DeleteTrackingPageCommandHandler : IRequestHandler<DeleteTrackingPageCommand, Unit>
{
    private readonly ITrackingPageRepository _trackingPageRepository;

    public DeleteTrackingPageCommandHandler(ITrackingPageRepository trackingPageRepository)
    {
        _trackingPageRepository = trackingPageRepository;
    }

    public async Task<Unit> Handle(DeleteTrackingPageCommand request, CancellationToken cancellationToken)
    {
        var aggregate = await _trackingPageRepository.GetByIdAsync(request.TrackingPageId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tracking page '{request.TrackingPageId}' was not found.");

        await _trackingPageRepository.DeleteAsync(aggregate, cancellationToken);

        return Unit.Value;
    }
}
