using Marky.Framework.Domain;
using Microsoft.EntityFrameworkCore;

namespace Marky.Framework.Persistence.EntityFramework.Repository;

public class Repository<T, TContext>(TContext context) : IRepository<T>
    where T : class, IAggregateRoot
    where TContext : DbContext
{
    protected readonly TContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet.FindAsync([id], ct);
    }

    public async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await DbSet.AddAsync(entity, ct);
    }

    public void Update(T entity)
    {
        DbSet.Update(entity);
    }

    public void Remove(T entity)
    {
        DbSet.Remove(entity);
    }
}
