using QPhising.Application.Contracts.Abstractions.Persistence;
using QPhising.Application.Contracts.Responses.Tracking;

namespace QPhising.Application.CQRS.Commands.Tracking;

public sealed record ArchiveTrackingPageCommand(Guid TrackingPageId) : ITransactionalRequest<TrackingPageResult>;
