namespace Marky.Framework.Persistence.Abstraction;

public interface IUnitOfWorkManager
{
    /// <summary>
    /// Starts a brand new data consistency boundary.
    /// </summary>
    public Task<IUnitOfWorkScope> BeginScopeAsync(CancellationToken ct = default);
}
