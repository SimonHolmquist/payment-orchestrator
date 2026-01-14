using MediatR;
using PaymentOrchestrator.Application.Common.Abstractions;

namespace PaymentOrchestrator.Application.Payments.Commands.CancelPayment;

public record CancelPaymentCommand(Guid PaymentId) : ITransactionalRequest<Unit>;
