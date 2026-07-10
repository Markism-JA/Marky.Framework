using Marky.Toolkit.Migration.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Marky.Toolkit.Migration.Strategies;

public class DropAndRecreateStrategy<TContext>(ILogger<DropAndRecreateStrategy<TContext>> logger)
    : IDbInitializationStrategy<TContext>
    where TContext : DbContext
{
    public async Task PrepareDatabaseAsync(
        TContext context,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogWarning(
            "SANDBOX INTERCEPTOR: Wiping and generating fresh schema via EnsureCreated..."
        );
        await context.Database.EnsureDeletedAsync(cancellationToken);
        await context.Database.EnsureCreatedAsync(cancellationToken);
    }
}
