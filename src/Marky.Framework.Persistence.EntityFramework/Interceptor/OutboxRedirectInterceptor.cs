using System.Text.Json;
using Marky.Framework.Persistence.EntityFramework.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Marky.Framework.Persistence.EntityFramework.Interceptor;

public class OutboxRedirectInterceptor : SaveChangesInterceptor
{
    private DbContext? _primaryContext;

    public void RedirectTo(DbContext primaryContext) => _primaryContext = primaryContext;

    public void ClearRedirect() => _primaryContext = null;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        if (_primaryContext is null || eventData.Context is null)
        {
            return base.SavingChanges(eventData, result);
        }

        ExecuteRedirect(eventData.Context);

        return InterceptionResult<int>.SuppressWithResult(0);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        if (_primaryContext is null || eventData.Context is null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        ExecuteRedirect(eventData.Context);

        return InterceptionResult<int>.SuppressWithResult(0);
    }

    private void ExecuteRedirect(DbContext secondaryContext)
    {
        var trackedEntries = secondaryContext.ChangeTracker.Entries().ToList();
        var recordsToAppend = new List<OutboxRecord>();

        foreach (var entry in trackedEntries)
        {
            if (entry.State is EntityState.Detached or EntityState.Unchanged)
                continue;

            recordsToAppend.Add(
                new OutboxRecord
                {
                    Type = entry.Entity.GetType().FullName ?? entry.Entity.GetType().Name,
                    Payload = JsonSerializer.Serialize(entry.Entity),
                    Operation = entry.State.ToString(),
                    OccurredOnUtc = DateTime.UtcNow,
                }
            );
        }

        if (recordsToAppend.Count == 0)
            return;

        _primaryContext!.Set<OutboxRecord>().AddRange(recordsToAppend);

        secondaryContext.ChangeTracker.Clear();
    }
}
