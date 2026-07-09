using Marky.Framework.Domain;

namespace Marky.Framework.Domain;

/// <summary>
/// Defines the generic contract for a repository, providing basic CRUD and querying operations
/// for domain entities within the Backend system.
/// </summary>
/// <typeparam name="T">The type of the domain entity. Must be an "IAggregateRoot".</typeparam>
public interface IRepository<T>
    where T : IAggregateRoot
{
    /// <summary>
    /// Retrieves a single entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="ct">A token to observe while waiting for the task to complete.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Begins tracking the specified entity to be inserted into the database upon the next SaveChanges.
    /// </summary>
    public Task AddAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// Marks an existing entity for deletion in the unit of work.
    /// </summary>
    /// <remarks>
    /// This method is synchronous as it only modifies the state in the change tracker.
    /// </remarks>
    public void Remove(T entity);

    /// <summary>
    /// Marks an entity as modified in the change tracker.
    /// </summary>
    /// <remarks>
    /// Most EF Core implementations do not require this if the entity was
    /// tracked during retrieval, but it is included for explicit updates.
    /// </remarks>
    public void Update(T entity);
}
