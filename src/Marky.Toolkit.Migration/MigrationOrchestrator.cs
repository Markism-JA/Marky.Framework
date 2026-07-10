using Marky.Toolkit.Migration.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Marky.Toolkit.Migration;

public class MigrationOrchestrator<TContext>(
    TContext context,
    IDbInitializationStrategy<TContext> initializationStrategy,
    IEnumerable<ISeedingStrategy<TContext>> seedingStrategies,
    ILogger<MigrationOrchestrator<TContext>> logger
) : IMigrationOrchestrator<TContext>
    where TContext : DbContext
{
    public async Task OrchestrateAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Starting database orchestration pipeline for assembly context: {ContextName}",
            typeof(TContext).Name
        );

        await initializationStrategy.PrepareDatabaseAsync(context, cancellationToken);
        foreach (var strategy in seedingStrategies)
        {
            logger.LogInformation(
                "Executing seed injection profile: {StrategyName}",
                strategy.GetType().Name
            );
            await strategy.SeedAsync(context, cancellationToken);
        }

        logger.LogInformation("Database orchestration pipeline completed successfully.");
    }
}
