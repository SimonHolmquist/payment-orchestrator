using PaymentOrchestrator.Domain.Common;

namespace PaymentOrchestrator.Domain.Payments.Events;

public sealed record PaymentAuthorizationRequested(PaymentId PaymentId, DateTimeOffset OccurredAt) : IDomainEvent;