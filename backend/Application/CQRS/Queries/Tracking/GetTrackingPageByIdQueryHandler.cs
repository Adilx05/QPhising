using MediatR;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Application.Contracts.Responses.Tracking;
using QPhising.Application.Mapping.Tracking;

namespace QPhising.Application.CQRS.Queries.Tracking;

public sealed class GetTrackingPageByIdQueryHandler : IRequestHandler<GetTrackingPageByIdQuery, TrackingPageResult>
{
    private readonly ITrackingPageRepository _trackingPageRepository;

    public GetTrackingPageByIdQueryHandler(ITrackingPageRepository trackingPageRepository)
    {
        _trackingPageRepository = trackingPageRepository;
    }

    public async Task<TrackingPageResult> Handle(GetTrackingPageByIdQuery request, CancellationToken cancellationToken)
    {
        var aggregate = await _trackingPageRepository.GetByIdAsync(request.TrackingPageId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tracking page '{request.TrackingPageId}' was not found.");

        return aggregate.ToResult();
    }
}
