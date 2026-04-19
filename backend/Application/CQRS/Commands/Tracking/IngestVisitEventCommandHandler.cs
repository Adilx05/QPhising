using MediatR;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Application.Contracts.Responses.Tracking;
using QPhising.Domain.Tracking.Entities;
using QPhising.Domain.Tracking.ValueObjects;

namespace QPhising.Application.CQRS.Commands.Tracking;

public sealed class IngestVisitEventCommandHandler : IRequestHandler<IngestVisitEventCommand, VisitIngestionResult>
{
    private readonly ITrackingPageRepository _trackingPageRepository;
    private readonly IVisitEventRepository _visitEventRepository;

    public IngestVisitEventCommandHandler(ITrackingPageRepository trackingPageRepository, IVisitEventRepository visitEventRepository)
    {
        _trackingPageRepository = trackingPageRepository;
        _visitEventRepository = visitEventRepository;
    }

    public async Task<VisitIngestionResult> Handle(IngestVisitEventCommand request, CancellationToken cancellationToken)
    {
        var trackingPage = await _trackingPageRepository.GetByIdAsync(request.TrackingPageId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tracking page '{request.TrackingPageId}' was not found.");

        var deduplicationWindow = TimeSpan.FromSeconds(request.DeduplicationWindowSeconds);
        var isDuplicate = await _visitEventRepository.ExistsDuplicateAsync(
            trackingPage.Id,
            request.SessionId,
            request.VisitorFingerprint,
            request.OccurredAtUtc,
            deduplicationWindow,
            cancellationToken);

        if (isDuplicate)
        {
            return new VisitIngestionResult(Guid.Empty, trackingPage.Id, true, DateTimeOffset.UtcNow);
        }

        var visitEvent = new VisitEventEntity(
            Guid.NewGuid(),
            trackingPage.Id,
            request.OccurredAtUtc,
            new TrackingIdentifier(request.SessionId),
            new TrackingIdentifier(request.VisitorFingerprint),
            request.UserAgent,
            request.ReferrerUrl,
            request.IpHash,
            request.IpAddressHashPolicy);

        await _visitEventRepository.SaveAsync(visitEvent, cancellationToken);

        return new VisitIngestionResult(visitEvent.Id, trackingPage.Id, false, DateTimeOffset.UtcNow);
    }
}
