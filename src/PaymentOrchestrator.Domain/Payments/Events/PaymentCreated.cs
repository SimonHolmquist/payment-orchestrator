using PaymentOrchestrator.Domain.Common;
using PaymentOrchestrator.Domain.Payments.ValueObjects;

namespace PaymentOrchestrator.Domain.Payments.Events;

public sealed record PaymentCreated(
    PaymentId PaymentId,
    string ClientId,
    Money Amount,
    DateTimeOffset OccurredAt
) : IDomainEvent;
