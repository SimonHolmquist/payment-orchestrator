using System.ComponentModel.DataAnnotations;

namespace PaymentOrchestrator.Infrastructure.Persistence.Entities;

public sealed class PaymentLedgerEntry
{
    public Guid Id { get; set; }

    public Guid PaymentId { get; set; }

    [MaxLength(50)]
    public string Action { get; set; } = default!; // Ej: PaymentAuthorized

    public DateTimeOffset OccurredAt { get; set; }

    // Snapshots del estado (serializados en JSON)
    public string? StateSnapshot { get; set; }

    // Metadatos de auditoría
    [MaxLength(100)]
    public string? CorrelationId { get; set; }
}