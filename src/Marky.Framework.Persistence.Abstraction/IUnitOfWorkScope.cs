namespace Marky.Framework.Persistence.Abstraction;

/// <summary>
/// Coordinates the persistence of multiple changes as a single atomic transaction.
/// This prevents data inconsistency by ensuring that either all changes succeed, or none are applied.
/// </summary>
public interface IUnitOfWorkScope : IAsyncDisposable
{
    /// <summary>
    /// Flushes all changes across all tracked data stores and commits the transaction.
    /// </summary>
    public Task CommitAsync(CancellationToken ct = default);

    /// <summary>
    /// Explicitly aborts the changes, though letting Dispose drop it handles this implicitly.
    /// </summary>
    public Task RollbackAsync(CancellationToken ct = default);
}
