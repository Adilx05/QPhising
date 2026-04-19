using MediatR;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Application.Contracts.Responses.Tracking;
using QPhising.Application.Mapping.Tracking;

namespace QPhising.Application.CQRS.Queries.Tracking;

public sealed class ListTrackingPagesQueryHandler : IRequestHandler<ListTrackingPagesQuery, IReadOnlyCollection<TrackingPageResult>>
{
    private readonly ITrackingPageRepository _trackingPageRepository;

    public ListTrackingPagesQueryHandler(ITrackingPageRepository trackingPageRepository)
    {
        _trackingPageRepository = trackingPageRepository;
    }

    public async Task<IReadOnlyCollection<TrackingPageResult>> Handle(ListTrackingPagesQuery request, CancellationToken cancellationToken)
    {
        var trackingPages = await _trackingPageRepository.ListAsync(cancellationToken);
        return trackingPages.Select(page => page.ToResult()).ToArray();
    }
}
