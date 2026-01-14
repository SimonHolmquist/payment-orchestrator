using MediatR;
using PaymentOrchestrator.Application.Common.Abstractions;

namespace PaymentOrchestrator.Application.Payments.Commands.CapturePayment;

public record CapturePaymentCommand(
    Guid PaymentId,
    decimal Amount
) : ITransactionalRequest<Unit>;
