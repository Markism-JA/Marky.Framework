using Microsoft.EntityFrameworkCore;

namespace Marky.Toolkit.Migration.Abstractions;

/// <summary>
/// Defines the execution blueprint for preparing a database state.
/// </summary>
public interface IDbInitializationStrategy<in TContext>
    where TContext : DbContext
{
    public Task PrepareDatabaseAsync(
        TContext context,
        CancellationToken cancellationToken = default
    );
}
