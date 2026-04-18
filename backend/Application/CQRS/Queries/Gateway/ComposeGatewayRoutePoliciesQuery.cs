using MediatR;
using QPhising.Application.Contracts.Responses.Gateway;

namespace QPhising.Application.CQRS.Queries.Gateway;

public sealed record ComposeGatewayRoutePoliciesQuery : IRequest<GatewayRoutePolicyCompositionResult>;
