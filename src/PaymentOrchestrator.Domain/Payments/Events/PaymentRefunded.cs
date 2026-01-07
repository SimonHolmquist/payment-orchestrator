using PaymentOrchestrator.Domain.Common;

namespace PaymentOrchestrator.Domain.Payments.Events;

public sealed record PaymentRefunded(
    PaymentId PaymentId,
    decimal RefundedNow,
    decimal RefundedTotal,
    PaymentStatus Status,
    DateTimeOffset OccurredAt
) : IDomainEvent;
