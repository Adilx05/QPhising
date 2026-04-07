namespace QPhising.Application.Common.Abstractions.Setup;

public interface IDatabaseSetupValidator
{
    Task<(bool IsValid, string Message)> ValidateAsync(CancellationToken cancellationToken = default);
}
