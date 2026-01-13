using MediatR;

namespace PaymentOrchestrator.Application.Payments.Commands.CreatePayment;

// El comando retorna el ID del pago creado
public record CreatePaymentCommand(
    string ClientId,
    decimal Amount,
    string Currency
) : IRequest<Guid>;
