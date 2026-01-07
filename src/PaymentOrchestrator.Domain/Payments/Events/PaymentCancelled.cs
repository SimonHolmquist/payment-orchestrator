using PaymentOrchestrator.Domain.Common;

namespace PaymentOrchestrator.Domain.Payments.Events;

public sealed record PaymentCancelled(
    PaymentId PaymentId,
    DateTimeOffset OccurredAt
) : IDomainEvent;
