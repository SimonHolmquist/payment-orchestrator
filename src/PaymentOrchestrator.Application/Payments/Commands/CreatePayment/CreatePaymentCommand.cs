using PaymentOrchestrator.Application.Common.Abstractions;

namespace PaymentOrchestrator.Application.Payments.Commands.CreatePayment;

// El comando retorna el ID del pago creado
public record CreatePaymentCommand(
    string ClientId,
    decimal Amount,
    string Currency
) : ITransactionalRequest<Guid>;
