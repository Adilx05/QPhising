using Npgsql;
using System.Net.Sockets;

namespace QPhising.Infrastructure.Persistence;

internal static class DatabaseSetupErrorClassifier
{
    internal static string Classify(Exception exception)
    {
        if (TryFindPostgresException(exception, out PostgresException? postgresException))
        {
            if (postgresException.SqlState is "28P01" or "28000")
            {
                return "auth";
            }

            if (postgresException.SqlState == "3D000")
            {
                return "db_not_found";
            }
        }

        if (exception is SocketException or TimeoutException || exception.InnerException is SocketException or TimeoutException)
        {
            return "network";
        }

        if (exception is NpgsqlException npgsqlException && npgsqlException.InnerException is SocketException)
        {
            return "network";
        }

        return "unknown";
    }

    private static bool TryFindPostgresException(Exception exception, out PostgresException? postgresException)
    {
        postgresException = exception as PostgresException;
        if (postgresException is not null)
        {
            return true;
        }

        if (exception.InnerException is null)
        {
            return false;
        }

        return TryFindPostgresException(exception.InnerException, out postgresException);
    }
}
