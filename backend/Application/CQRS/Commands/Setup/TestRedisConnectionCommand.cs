using MediatR;
using QPhising.Application.Contracts.Responses.Setup;

namespace QPhising.Application.CQRS.Commands.Setup;

public sealed record TestRedisConnectionCommand(string ConnectionString) : IRequest<SetupDependencyTestResult>;
