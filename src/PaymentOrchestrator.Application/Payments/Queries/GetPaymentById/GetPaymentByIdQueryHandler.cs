using MediatR;
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Domain.Payments;

namespace PaymentOrchestrator.Application.Payments.Queries.GetPaymentById;

public class GetPaymentByIdQueryHandler(IPaymentRepository paymentRepository)
    : IRequestHandler<GetPaymentByIdQuery, PaymentDto?>
{
    public async Task<PaymentDto?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var paymentId = new PaymentId(request.Id);
        var payment = await paymentRepository.GetByIdAsync(paymentId, cancellationToken);

        if (payment is null) return null;

        // Mapeo manual de Dominio a DTO (se podría usar Mapster/AutoMapper)
        return new PaymentDto(
            payment.Id.Value,
            payment.ClientId,
            payment.Amount.Amount,
            payment.Amount.Currency,
            payment.Status.ToString(),
            payment.PspReference,
            payment.FailureReason,
            payment.CreatedAt
        );
    }
}