using PaymentOrchestrator.Domain.Common;
using PaymentOrchestrator.Domain.Payments.ValueObjects;

namespace PaymentOrchestrator.Domain.Payments.Events;

[EventName("payment.created.v1")]
public sealed record PaymentCreated(
    PaymentId PaymentId,
    string ClientId,
    Money Amount,
    DateTimeOffset OccurredAt
) : IDomainEvent;
