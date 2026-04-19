using MediatR;
using QPhising.Application.Contracts.Responses.Tracking;

namespace QPhising.Application.CQRS.Queries.Tracking;

public sealed record GetTrackingPageByIdQuery(Guid TrackingPageId) : IRequest<TrackingPageResult>;
