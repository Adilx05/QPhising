using MediatR;
using QPhising.Application.Contracts.Abstractions.Persistence;

namespace QPhising.Application.CQRS.Commands.Tracking;

public sealed record DeleteTrackingPageCommand(Guid TrackingPageId) : ITransactionalRequest<Unit>;
