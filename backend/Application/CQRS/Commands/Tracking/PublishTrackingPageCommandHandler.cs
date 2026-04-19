using MediatR;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Application.Contracts.Responses.Tracking;
using QPhising.Application.Mapping.Tracking;

namespace QPhising.Application.CQRS.Commands.Tracking;

public sealed class PublishTrackingPageCommandHandler : IRequestHandler<PublishTrackingPageCommand, TrackingPageResult>
{
    private readonly ITrackingPageRepository _trackingPageRepository;

    public PublishTrackingPageCommandHandler(ITrackingPageRepository trackingPageRepository)
    {
        _trackingPageRepository = trackingPageRepository;
    }

    public async Task<TrackingPageResult> Handle(PublishTrackingPageCommand request, CancellationToken cancellationToken)
    {
        var aggregate = await _trackingPageRepository.GetByIdAsync(request.TrackingPageId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tracking page '{request.TrackingPageId}' was not found.");

        aggregate.Publish();

        await _trackingPageRepository.SaveAsync(aggregate, cancellationToken);

        return aggregate.ToResult();
    }
}
