using MediatR;
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Domain.Payments;
using PaymentOrchestrator.Domain.Payments.ValueObjects;

namespace PaymentOrchestrator.Application.Payments.Commands.CreatePayment;

public class CreatePaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    IClock clock)
    : IRequestHandler<CreatePaymentCommand, Guid>
{
    public async Task<Guid> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        // 1. Convertir DTOs a Value Objects del Dominio
        var paymentId = PaymentId.New();
        var money = Money.Of(request.Amount, request.Currency);

        // 2. Ejecutar lógica de dominio puro
        // Nota: Asumimos DateTimeOffset.UtcNow. En un entorno real, inyectaríamos IClock.
        var payment = Payment.Create(paymentId, request.ClientId, money, clock.UtcNow);

        // 3. Persistir cambios
        await paymentRepository.AddAsync(payment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return paymentId.Value;
    }
}