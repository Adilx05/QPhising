using MediatR;
using QPhising.Application.Contracts.Responses.Tracking;

namespace QPhising.Application.CQRS.Queries.Tracking;

public sealed record ListTrackingPagesQuery : IRequest<IReadOnlyCollection<TrackingPageResult>>;
