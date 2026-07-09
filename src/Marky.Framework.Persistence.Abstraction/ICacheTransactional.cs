namespace Marky.Framework.Persistence.Abstraction;

/// <summary>
/// Infrastructure contract providing transactional synchronization hooks for external caching structures.
/// </summary>
public interface ICacheTransactional
{
    /// <summary>
    /// Flushes all buffered cache operations inside a high-performance network pipeline handshake.
    /// </summary>
    public Task FlushChangesAsync();

    /// <summary>
    /// Purges all uncommitted memory queues instantly, wiping any dirty operations during rollbacks.
    /// </summary>
    public void ClearBufferedChanges();
}
