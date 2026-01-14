using MediatR;
using PaymentOrchestrator.Application.Common.Abstractions;

namespace PaymentOrchestrator.Application.Payments.Commands.RefundPayment;

public record RefundPaymentCommand(
    Guid PaymentId,
    decimal Amount
) : ITransactionalRequest<Unit>;
