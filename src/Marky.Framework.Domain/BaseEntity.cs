namespace Marky.Framework.Domain;

public abstract class BaseEntity : IAuditable
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
