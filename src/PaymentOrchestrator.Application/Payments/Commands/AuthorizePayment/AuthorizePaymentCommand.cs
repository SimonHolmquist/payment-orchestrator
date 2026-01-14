using MediatR;
using PaymentOrchestrator.Application.Common.Abstractions;

namespace PaymentOrchestrator.Application.Payments.Commands.AuthorizePayment;

public record AuthorizePaymentCommand(
    Guid PaymentId,
    string PspReference
) : ITransactionalRequest<Unit>;
