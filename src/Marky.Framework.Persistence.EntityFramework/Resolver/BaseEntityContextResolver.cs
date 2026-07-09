using Marky.Framework.Persistence.Abstraction;

namespace Marky.Framework.Persistence.EntityFramework.Resolver;

public abstract class BaseEntityContextResolver : IEntityContextResolver
{
    protected abstract IReadOnlyDictionary<Type, Type> EntityContextMap { get; }

    public Type ResolveContextType(Type entityType)
    {
        if (!EntityContextMap.TryGetValue(entityType, out var contextType))
        {
            throw new InvalidOperationException(
                $"[{GetType().Name}] No DbContext registered for domain entity type: {entityType.FullName}"
            );
        }

        return contextType;
    }
}
