using Marky.Framework.Domain;
using Marky.Framework.Persistence.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace Marky.Framework.Persistence.EntityFramework.Repository;

public class DynamicRepositoryProxy<T>(
    IServiceProvider serviceProvider,
    IEntityContextResolver contextResolver
) : IRepository<T>
    where T : class, IAggregateRoot
{
    private IRepository<T>? _innerRepository;

    private IRepository<T> GetRepository()
    {
        if (_innerRepository != null)
            return _innerRepository;

        var contextType = contextResolver.ResolveContextType(typeof(T));

        var dbContext = serviceProvider.GetRequiredService(contextType);

        var repoType = typeof(Repository<,>).MakeGenericType(typeof(T), contextType);

        _innerRepository = (IRepository<T>)Activator.CreateInstance(repoType, dbContext)!;
        return _innerRepository;
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        GetRepository().GetByIdAsync(id, ct);

    public Task AddAsync(T entity, CancellationToken ct = default) =>
        GetRepository().AddAsync(entity, ct);

    public void Update(T entity) => GetRepository().Update(entity);

    public void Remove(T entity) => GetRepository().Remove(entity);
}
