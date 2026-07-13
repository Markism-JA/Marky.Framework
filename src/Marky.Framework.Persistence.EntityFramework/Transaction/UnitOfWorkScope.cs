using Marky.Framework.Persistence.Abstraction;
using Marky.Framework.Persistence.EntityFramework.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Marky.Framework.Persistence.EntityFramework.Transaction;

public class UnitOfWorkScope(
    List<DbContext> contexts,
    List<IDbContextTransaction> transactions,
    IEnumerable<ICacheTransactional> transactionalCaches,
    OutboxRedirectState redirectState
) : IUnitOfWorkScope
{
    private readonly OutboxRedirectState _redirectState = redirectState;

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
                cache.ClearBufferedChanges();
            }
            throw;
        }
        finally
        {
            _redirectState.DisableRedirect();
        }

        // Database verified safe. Execute parallel cache execution pipelines.
        var flushTasks = transactionalCaches.Select(cache => cache.FlushChangesAsync());
        await Task.WhenAll(flushTasks);
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        try
        {
            foreach (var transaction in transactions)
            {
                await transaction.RollbackAsync(ct);
            }
        }
        finally
        {
            _redirectState.DisableRedirect();
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
        _redirectState?.DisableRedirect();
        foreach (var transaction in transactions)
        {
            await transaction.DisposeAsync();
        }
        GC.SuppressFinalize(this);
    }
}
