using PaymentOrchestrator.Domain.Common;

namespace PaymentOrchestrator.Domain.Payments.Events;

public sealed record PaymentCaptured(
    PaymentId PaymentId,
    decimal CapturedNow,
    decimal CapturedTotal,
    PaymentStatus Status,
    DateTimeOffset OccurredAt
) : IDomainEvent;
