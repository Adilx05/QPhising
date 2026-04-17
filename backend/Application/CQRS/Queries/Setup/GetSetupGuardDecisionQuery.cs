using MediatR;
using QPhising.Application.Contracts.Responses.Setup;

namespace QPhising.Application.CQRS.Queries.Setup;

public sealed record GetSetupGuardDecisionQuery : IRequest<SetupGuardDecisionResult>;
