using MediatR;
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Domain.Common;
using PaymentOrchestrator.Domain.Payments;

namespace PaymentOrchestrator.Application.Payments.Commands.AuthorizePayment;

public class AuthorizePaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    IClock clock)
    : IRequestHandler<AuthorizePaymentCommand, Unit>
{
    public async Task<Unit> Handle(AuthorizePaymentCommand request, CancellationToken cancellationToken)
    {
        var paymentId = new PaymentId(request.PaymentId);
        var payment = await paymentRepository.GetByIdAsync(paymentId, cancellationToken) ?? throw new DomainException($"Payment {request.PaymentId} not found.");

        // Ejecutar lógica de dominio
        payment.MarkAuthorized(request.PspReference, clock.UtcNow);

        // Persistir (EF Core detecta cambios automáticamente)
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}