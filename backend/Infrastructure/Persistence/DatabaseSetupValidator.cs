using Microsoft.EntityFrameworkCore;
using QPhising.Application.Common.Abstractions.Setup;

namespace QPhising.Infrastructure.Persistence;

public sealed class DatabaseSetupValidator(QPhisingDbContext dbContext) : IDatabaseSetupValidator
{
    public async Task<(bool IsValid, string Message)> ValidateAsync(CancellationToken cancellationToken = default)
    {
        bool canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        if (!canConnect)
        {
            return (false, "Database connection failed.");
        }

        int pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Count();
        return pendingMigrations == 0
            ? (true, "Database connection is healthy and migrations are up to date.")
            : (false, $"Database connection is healthy but {pendingMigrations} pending migration(s) found.");
    }
}
