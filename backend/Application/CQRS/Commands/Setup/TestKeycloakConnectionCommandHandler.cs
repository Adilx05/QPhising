using MediatR;
using QPhising.Application.Contracts.Abstractions.Setup;
using QPhising.Application.Contracts.Responses.Setup;

namespace QPhising.Application.CQRS.Commands.Setup;

public sealed class TestKeycloakConnectionCommandHandler : IRequestHandler<TestKeycloakConnectionCommand, SetupDependencyTestResult>
{
    private readonly ISetupDependencyConnectionTester _setupDependencyConnectionTester;

    public TestKeycloakConnectionCommandHandler(ISetupDependencyConnectionTester setupDependencyConnectionTester)
    {
        _setupDependencyConnectionTester = setupDependencyConnectionTester;
    }

    public Task<SetupDependencyTestResult> Handle(TestKeycloakConnectionCommand request, CancellationToken cancellationToken) =>
        _setupDependencyConnectionTester.TestKeycloakAsync(
            new Uri(request.Authority),
            request.Realm,
            request.ClientId,
            request.ClientSecret,
            cancellationToken);
}
