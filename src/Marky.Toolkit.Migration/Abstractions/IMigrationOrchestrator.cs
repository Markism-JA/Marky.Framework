using Microsoft.EntityFrameworkCore;

namespace Marky.Toolkit.Migration.Abstractions;

/// <summary>
/// Handles the runtime deployment lifecycle of database schemas.
/// </summary>
public interface IMigrationOrchestrator<in TContext>
    where TContext : DbContext
{
    /// <summary>
    /// Evaluates pending deltas and safely applies migrations and seeds to the database.
    /// </summary>
    public Task OrchestrateAsync(CancellationToken cancellationToken = default);
}
