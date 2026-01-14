using System.ComponentModel.DataAnnotations;

namespace PaymentOrchestrator.Infrastructure.Persistence.Entities;

public sealed class InboxMessage
{
    public Guid Id { get; set; }

    [MaxLength(50)]
    public string Provider { get; set; } = default!;

    [MaxLength(200)]
    public string ProviderEventId { get; set; } = default!;

    [MaxLength(100)]
    public string EventType { get; set; } = default!;

    public string RawPayload { get; set; } = default!;

    public DateTimeOffset ReceivedAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }

    public string? Error { get; set; }
}
