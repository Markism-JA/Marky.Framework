using System.Text.Json;
using System.Text.Json.Serialization;
using Marky.Framework.Persistence.EntityFramework.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Marky.Framework.Persistence.EntityFramework.Interceptor;

public class OutboxRedirectInterceptor : SaveChangesInterceptor
{
    private readonly OutboxRedirectState _state;
    private readonly JsonSerializerOptions _serializerOptions;

    public OutboxRedirectInterceptor(OutboxRedirectState state)
    {
        _state = state;
        _serializerOptions = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = false,
        };
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        if (
            _state.PrimaryContext is null
            || eventData.Context is null
            || eventData.Context == _state.PrimaryContext
        )
        {
            return base.SavingChanges(eventData, result);
        }

        ExecuteRedirect(eventData.Context, _state.PrimaryContext);

        return InterceptionResult<int>.SuppressWithResult(0);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        if (
            _state.PrimaryContext is null
            || eventData.Context is null
            || eventData.Context == _state.PrimaryContext
        )
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        ExecuteRedirect(eventData.Context, _state.PrimaryContext);
        return InterceptionResult<int>.SuppressWithResult(0);
    }

    private void ExecuteRedirect(DbContext secondaryContext, DbContext primaryContext)
    {
        var trackedEntries = secondaryContext.ChangeTracker.Entries().ToList();
        var recordsToAppend = new List<OutboxRecord>();

        foreach (var entry in trackedEntries)
        {
            if (entry.State is EntityState.Detached or EntityState.Unchanged)
                continue;

            var propertyValues = entry.Properties.ToDictionary(
                p => p.Metadata.Name,
                p => p.CurrentValue
            );

            recordsToAppend.Add(
                new OutboxRecord
                {
                    Type = entry.Entity.GetType().FullName ?? entry.Entity.GetType().Name,
                    Payload = JsonSerializer.Serialize(propertyValues, _serializerOptions),
                    Operation = entry.State.ToString(),
                    OccurredOnUtc = DateTime.UtcNow,
                }
            );
        }

        if (recordsToAppend.Count == 0)
            return;

        primaryContext.Set<OutboxRecord>().AddRange(recordsToAppend);

        secondaryContext.ChangeTracker.Clear();
    }
}
