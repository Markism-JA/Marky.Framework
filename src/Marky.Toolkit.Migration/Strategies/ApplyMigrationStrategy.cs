using Marky.Toolkit.Migration.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Marky.Toolkit.Migration.Strategies;

public class ApplyMigrationsStrategy<TContext>(ILogger<ApplyMigrationsStrategy<TContext>> logger)
    : IDbInitializationStrategy<TContext>
    where TContext : DbContext
{
    public async Task PrepareDatabaseAsync(
        TContext context,
        CancellationToken cancellationToken = default
    )
    {
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
        var migrations = pendingMigrations as string[] ?? pendingMigrations.ToArray();

        if (migrations.Any())
        {
            logger.LogInformation(
                "Applying {Count} pending schema deltas to storage engine...",
                migrations.Length
            );
            await context.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            logger.LogInformation(
                "Storage engine tracking matches assembly schema state. No deltas applied."
            );
        }
    }
}
