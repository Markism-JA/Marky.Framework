using Marky.Framework.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Marky.Framework.Persistence.EntityFramework.Interceptor;

public class PersistenceAuditInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        if (eventData.Context is null)
            return base.SavingChanges(eventData, result);

        EvaluateAndAuditEntries(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void EvaluateAndAuditEntries(DbContext context)
    {
        var currentTime = timeProvider.GetUtcNow();
        var entries = context.ChangeTracker.Entries().ToList();

        foreach (var entry in entries)
        {
            if (entry is { State: EntityState.Deleted, Entity: ISoftDelete softDeleteEntity })
            {
                entry.State = EntityState.Modified;
                softDeleteEntity.IsDeleted = true;
                softDeleteEntity.DeletedAt = currentTime;
            }

            if (entry.Entity is IAuditable auditableEntity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        auditableEntity.CreatedAt = currentTime;
                        break;
                    case EntityState.Modified:
                        auditableEntity.UpdatedAt = currentTime;
                        break;
                }
            }
        }
    }
}
