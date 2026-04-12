using QPhising.Application.Common.Abstractions.Setup;

namespace QPhising.Application.Features.Setup;

public sealed record DatabaseConfigurationSnapshot(
    string? Host,
    int? Port,
    string? Database,
    string? Username,
    string? Password,
    string? ConnectionString)
{
    public static DatabaseConfigurationSnapshot FromInput(DatabaseConnectionInput input)
    {
        return new DatabaseConfigurationSnapshot(
            input.Host,
            input.Port,
            input.Database,
            input.Username,
            input.Password,
            input.ConnectionString);
    }
}
