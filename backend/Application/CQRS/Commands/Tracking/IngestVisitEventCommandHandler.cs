using MediatR;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Application.Contracts.Responses.Tracking;
using QPhising.Domain.Tracking.Entities;
using QPhising.Domain.Tracking.Enums;
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

        var sanitizedReferrerUrl = trackingPage.Settings?.CaptureUtmParameters == false
            ? RemoveUtmParameters(request.ReferrerUrl)
            : request.ReferrerUrl;

        if (trackingPage.Settings?.EnableBotFiltering == true && IsBotUserAgent(request.UserAgent))
        {
            return new VisitIngestionResult(Guid.Empty, trackingPage.Id, true, DateTimeOffset.UtcNow);
        }


        var settingsCaptureIp = trackingPage.Settings?.CaptureIpAddress ?? true;
        var settingsIpPolicy = trackingPage.Settings?.IpAddressHashPolicy ?? IpAddressHashPolicy.Sha256;

        if (!settingsCaptureIp)
        {
            settingsIpPolicy = IpAddressHashPolicy.None;
        }

        if (settingsCaptureIp && request.IpAddressHashPolicy != settingsIpPolicy)
        {
            throw new InvalidOperationException("Visit IP policy does not match tracking page privacy settings.");
        }

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
            sanitizedReferrerUrl,
            settingsCaptureIp ? request.IpHash : null,
            settingsIpPolicy);

        await _visitEventRepository.SaveAsync(visitEvent, cancellationToken);

        return new VisitIngestionResult(visitEvent.Id, trackingPage.Id, false, DateTimeOffset.UtcNow);
    }

    private static bool IsBotUserAgent(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return false;
        }

        var normalized = userAgent.Trim();
        return normalized.Contains("bot", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("spider", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("crawler", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("slurp", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("headless", StringComparison.OrdinalIgnoreCase);
    }

    private static string? RemoveUtmParameters(string? referrerUrl)
    {
        if (string.IsNullOrWhiteSpace(referrerUrl)
            || !Uri.TryCreate(referrerUrl, UriKind.Absolute, out var absoluteUri)
            || string.IsNullOrEmpty(absoluteUri.Query))
        {
            return referrerUrl;
        }

        var queryParameters = absoluteUri.Query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Where(parameter => !parameter.StartsWith("utm_", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (queryParameters.Length == 0)
        {
            return absoluteUri.GetLeftPart(UriPartial.Path);
        }

        var uriBuilder = new UriBuilder(absoluteUri)
        {
            Query = string.Join('&', queryParameters)
        };

        return uriBuilder.Uri.ToString();
    }
}
