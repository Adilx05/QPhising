using MediatR;
using QPhising.Application.Contracts.Responses.Setup;

namespace QPhising.Application.CQRS.Commands.Setup;

public sealed record TestKeycloakConnectionCommand(
    string Authority,
    string Realm,
    string ClientId,
    string ClientSecret) : IRequest<SetupDependencyTestResult>;
