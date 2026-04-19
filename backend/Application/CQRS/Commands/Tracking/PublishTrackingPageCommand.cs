using QPhising.Application.Contracts.Abstractions.Persistence;
using QPhising.Application.Contracts.Responses.Tracking;

namespace QPhising.Application.CQRS.Commands.Tracking;

public sealed record PublishTrackingPageCommand(Guid TrackingPageId) : ITransactionalRequest<TrackingPageResult>;
