using Microsoft.EntityFrameworkCore;

namespace Marky.Toolkit.Migration.Abstractions;

/// <summary>
/// Decouples initial static data populating logic from structural schemas.
/// </summary>
public interface ISeedingStrategy<in TContext>
    where TContext : DbContext
{
    /// <summary>
    /// Executes idempotent data seeding operations against the context.
    /// </summary>
    public Task SeedAsync(TContext context, CancellationToken cancellationToken = default);
}
