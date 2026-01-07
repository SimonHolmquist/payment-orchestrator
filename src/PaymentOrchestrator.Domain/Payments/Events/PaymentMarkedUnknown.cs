using PaymentOrchestrator.Domain.Common;

namespace PaymentOrchestrator.Domain.Payments.Events;

public sealed record PaymentMarkedUnknown(
    PaymentId PaymentId,
    string Reason,
    DateTimeOffset OccurredAt
) : IDomainEvent;
