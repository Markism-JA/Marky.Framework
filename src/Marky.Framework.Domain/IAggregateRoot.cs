namespace Marky.Framework.Domain;

/// <summary>
/// A marker interface identifying an entity as an Aggregate Root.
/// </summary>
/// <remarks>
/// Aggregate Roots are the only entities that should be loaded directly from repositories.
/// They maintain the consistency of the entire cluster of objects (entities and value objects)
/// within their boundary.
/// </remarks>
public interface IAggregateRoot { }
