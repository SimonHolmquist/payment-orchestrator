using MediatR;
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Domain.Common;
using PaymentOrchestrator.Domain.Payments;

namespace PaymentOrchestrator.Application.Payments.Commands.CancelPayment;

public class CancelPaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    IClock clock)
    : IRequestHandler<CancelPaymentCommand, Unit>
{
    public async Task<Unit> Handle(CancelPaymentCommand request, CancellationToken cancellationToken)
    {
        var paymentId = new PaymentId(request.PaymentId);
        var payment = await paymentRepository.GetByIdAsync(paymentId, cancellationToken) ?? throw new DomainException($"Payment {request.PaymentId} not found.");
        payment.Cancel(clock.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}