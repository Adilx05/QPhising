namespace QPhising.Application.Common.Abstractions.Setup;

public interface ISsoSetupValidator
{
    Task<(bool IsValid, string Message)> ValidateAsync(CancellationToken cancellationToken = default);
}
