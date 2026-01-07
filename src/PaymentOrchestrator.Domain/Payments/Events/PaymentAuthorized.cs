using PaymentOrchestrator.Domain.Common;

namespace PaymentOrchestrator.Domain.Payments.Events;

public sealed record PaymentAuthorized(
    PaymentId PaymentId,
    string PspReference,
    DateTimeOffset OccurredAt
) : IDomainEvent;
