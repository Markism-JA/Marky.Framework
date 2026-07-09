namespace Marky.Framework.Persistence.Abstraction;

public interface IEntityContextResolver
{
    public Type ResolveContextType(Type entityType);
}
