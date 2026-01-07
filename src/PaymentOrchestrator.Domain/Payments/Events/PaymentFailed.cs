using PaymentOrchestrator.Domain.Common;

namespace PaymentOrchestrator.Domain.Payments.Events;

public sealed record PaymentFailed(
    PaymentId PaymentId,
    string Reason,
    DateTimeOffset OccurredAt
) : IDomainEvent;
