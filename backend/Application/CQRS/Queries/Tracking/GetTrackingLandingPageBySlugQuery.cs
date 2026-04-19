using MediatR;
using QPhising.Application.Contracts.Responses.Tracking;

namespace QPhising.Application.CQRS.Queries.Tracking;

public sealed record GetTrackingLandingPageBySlugQuery(string Slug, Guid? Id, string? Campaign) : IRequest<TrackingLandingPageResult>;
