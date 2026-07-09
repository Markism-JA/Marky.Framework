namespace Marky.Framework.Domain;

public interface IAuditable
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
