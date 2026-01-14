using MediatR;
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Domain.Common;
using PaymentOrchestrator.Domain.Payments;

namespace PaymentOrchestrator.Application.Payments.Commands.RefundPayment;

public class RefundPaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    IClock clock)
    : IRequestHandler<RefundPaymentCommand, Unit>
{
    public async Task<Unit> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var paymentId = new PaymentId(request.PaymentId);
        var payment = await paymentRepository.GetByIdAsync(paymentId, cancellationToken) ?? throw new DomainException($"Payment {request.PaymentId} not found.");
        payment.MarkRefund(request.Amount, clock.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}