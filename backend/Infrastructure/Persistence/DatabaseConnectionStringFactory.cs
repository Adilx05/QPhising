using Npgsql;
using QPhising.Application.Common.Abstractions.Setup;

namespace QPhising.Infrastructure.Persistence;

internal static class DatabaseConnectionStringFactory
{
    internal static string Build(DatabaseConnectionInput input)
    {
        if (!string.IsNullOrWhiteSpace(input.ConnectionString))
        {
            return input.ConnectionString;
        }

        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = input.Host,
            Port = input.Port ?? 5432,
            Database = input.Database,
            Username = input.Username,
            Password = input.Password,
            SslMode = SslMode.Prefer,
            Timeout = 5,
            CommandTimeout = 10
        };

        return builder.ConnectionString;
    }
}
