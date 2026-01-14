using PaymentOrchestrator.Domain.Common;

namespace PaymentOrchestrator.Domain.Payments.Events;

public sealed record PaymentCaptureRequested(PaymentId PaymentId, decimal Amount, DateTimeOffset OccurredAt) : IDomainEvent;