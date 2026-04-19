using MediatR;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Application.Contracts.Responses.Tracking;
using QPhising.Domain.Tracking.Enums;

namespace QPhising.Application.CQRS.Queries.Tracking;

public sealed class GetTrackingLandingPageBySlugQueryHandler : IRequestHandler<GetTrackingLandingPageBySlugQuery, TrackingLandingPageResult>
{
    private readonly ITrackingPageRepository _trackingPageRepository;

    public GetTrackingLandingPageBySlugQueryHandler(ITrackingPageRepository trackingPageRepository)
    {
        _trackingPageRepository = trackingPageRepository;
    }

    public async Task<TrackingLandingPageResult> Handle(GetTrackingLandingPageBySlugQuery request, CancellationToken cancellationToken)
    {
        var aggregate = await _trackingPageRepository.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException($"Tracking page slug '{request.Slug}' was not found.");

        if (aggregate.PublishState != TrackingPagePublishState.Published)
        {
            throw new KeyNotFoundException($"Tracking page slug '{request.Slug}' was not found.");
        }

        return new TrackingLandingPageResult(
            aggregate.Id,
            aggregate.Slug.Value,
            aggregate.Title,
            aggregate.Description,
            aggregate.DestinationUrl.Value);
    }
}
