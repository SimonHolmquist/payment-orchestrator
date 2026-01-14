namespace PaymentOrchestrator.Infrastructure.Persistence.Entities;

public sealed class OutboxMessage
{
    public Guid Id { get; set; }

    public string Type { get; set; } = default!;
    public string Content { get; set; } = default!;

    public Guid AggregateId { get; set; }
    public string AggregateType { get; set; } = default!;

    public string CorrelationId { get; set; } = default!;

    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }

    public int Attempts { get; set; }
    public string? Error { get; set; }
}
