using Marky.Framework.Persistence.Abstraction;
using Marky.Framework.Persistence.EntityFramework.Interceptor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Marky.Framework.Persistence.EntityFramework.Transaction;

public class UnitOfWorkScope(
    List<DbContext> contexts,
    List<IDbContextTransaction> transactions,
    IEnumerable<ICacheTransactional> transactionalCaches,
    OutboxRedirectInterceptor? interceptor
) : IUnitOfWorkScope
{
    public async Task CommitAsync(CancellationToken ct = default)
    {
        var contextsWithChanges = contexts.Where(c => c.ChangeTracker.HasChanges()).ToList();
        foreach (var context in contextsWithChanges)
        {
            await context.SaveChangesAsync(ct);
        }

        var committedTransactions = new List<IDbContextTransaction>();

        try
        {
            for (int i = 0; i < transactions.Count; i++)
            {
                await transactions[i].CommitAsync(ct);
                committedTransactions.Add(transactions[i]);
            }
        }
        catch (Exception)
        {
            await RollbackRemainingAsync(committedTransactions, ct);

            foreach (var cache in transactionalCaches)
            {
                // Assuming your interface exposes a way to clear or abort changes
                cache.ClearBufferedChanges();
            }
            throw;
        }
        finally
        {
            interceptor?.ClearRedirect();
        }

        // Database verified safe. Execute parallel cache execution pipelines.
        var flushTasks = transactionalCaches.Select(cache => cache.FlushChangesAsync());
        await Task.WhenAll(flushTasks);
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        foreach (var transaction in transactions)
        {
            await transaction.RollbackAsync(ct);
        }

        foreach (var cache in transactionalCaches)
        {
            cache.ClearBufferedChanges();
        }
    }

    private async Task RollbackRemainingAsync(
        List<IDbContextTransaction> committed,
        CancellationToken ct
    )
    {
        foreach (var transaction in transactions.Except(committed))
        {
            try
            {
                await transaction.RollbackAsync(ct);
            }
            catch
            { /* Suppress error tracking artifacts */
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        interceptor?.ClearRedirect();
        foreach (var transaction in transactions)
        {
            await transaction.DisposeAsync();
        }
        GC.SuppressFinalize(this);
    }
}
