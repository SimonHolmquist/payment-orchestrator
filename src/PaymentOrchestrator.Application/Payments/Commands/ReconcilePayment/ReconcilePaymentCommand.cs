using MediatR;
using PaymentOrchestrator.Application.Common.Abstractions;

namespace PaymentOrchestrator.Application.Payments.Commands.ReconcilePayment;

public sealed record ReconcilePaymentCommand(Guid PaymentId)
    : ITransactionalRequest<Unit>;
