namespace Marky.Framework.Persistence.EntityFramework.Outbox;

public class OutboxRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccurredOnUtc { get; set; }
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
    public bool IsFatal { get; set; }
    public DateTime? LastAttemptedOnUtc { get; set; }
    public string? Operation { get; set; }
}
