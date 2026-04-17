using MediatR;
using QPhising.Application.Contracts.Responses.Setup;

namespace QPhising.Application.CQRS.Commands.Setup;

public sealed record TestDatabaseConnectionCommand(string ConnectionString) : IRequest<SetupDependencyTestResult>;
