using MediatR;
using QPhising.Application.Contracts.Abstractions.Setup;
using QPhising.Application.Contracts.Responses.Setup;

namespace QPhising.Application.CQRS.Commands.Setup;

public sealed class TestRedisConnectionCommandHandler : IRequestHandler<TestRedisConnectionCommand, SetupDependencyTestResult>
{
    private readonly ISetupDependencyConnectionTester _setupDependencyConnectionTester;

    public TestRedisConnectionCommandHandler(ISetupDependencyConnectionTester setupDependencyConnectionTester)
    {
        _setupDependencyConnectionTester = setupDependencyConnectionTester;
    }

    public Task<SetupDependencyTestResult> Handle(TestRedisConnectionCommand request, CancellationToken cancellationToken) =>
        _setupDependencyConnectionTester.TestRedisAsync(request.ConnectionString, cancellationToken);
}
