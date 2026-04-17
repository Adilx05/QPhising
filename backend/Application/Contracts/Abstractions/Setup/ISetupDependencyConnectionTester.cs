using QPhising.Application.Contracts.Responses.Setup;

namespace QPhising.Application.Contracts.Abstractions.Setup;

public interface ISetupDependencyConnectionTester
{
    Task<SetupDependencyTestResult> TestDatabaseAsync(string connectionString, CancellationToken cancellationToken);

    Task<SetupDependencyTestResult> TestKeycloakAsync(
        Uri authority,
        string realm,
        string clientId,
        string clientSecret,
        CancellationToken cancellationToken);

    Task<SetupDependencyTestResult> TestRedisAsync(string connectionString, CancellationToken cancellationToken);
}
