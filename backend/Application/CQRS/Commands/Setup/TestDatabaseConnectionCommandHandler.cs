using MediatR;
using QPhising.Application.Contracts.Abstractions.Setup;
using QPhising.Application.Contracts.Responses.Setup;

namespace QPhising.Application.CQRS.Commands.Setup;

public sealed class TestDatabaseConnectionCommandHandler : IRequestHandler<TestDatabaseConnectionCommand, SetupDependencyTestResult>
{
    private readonly ISetupDependencyConnectionTester _setupDependencyConnectionTester;

    public TestDatabaseConnectionCommandHandler(ISetupDependencyConnectionTester setupDependencyConnectionTester)
    {
        _setupDependencyConnectionTester = setupDependencyConnectionTester;
    }

    public Task<SetupDependencyTestResult> Handle(TestDatabaseConnectionCommand request, CancellationToken cancellationToken) =>
        _setupDependencyConnectionTester.TestDatabaseAsync(request.ConnectionString, cancellationToken);
}
