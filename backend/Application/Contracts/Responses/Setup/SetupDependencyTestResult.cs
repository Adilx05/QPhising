namespace QPhising.Application.Contracts.Responses.Setup;

public sealed record SetupDependencyTestResult(
    string Dependency,
    bool Succeeded,
    string? FailureReason = null);
