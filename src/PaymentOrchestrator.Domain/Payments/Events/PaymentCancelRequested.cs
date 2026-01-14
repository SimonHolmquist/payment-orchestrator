using PaymentOrchestrator.Domain.Common;

namespace PaymentOrchestrator.Domain.Payments.Events;

public sealed record PaymentCancelRequested(PaymentId PaymentId, DateTimeOffset OccurredAt) : IDomainEvent;