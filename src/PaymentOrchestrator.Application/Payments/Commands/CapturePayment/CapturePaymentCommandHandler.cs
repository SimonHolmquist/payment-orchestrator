using MediatR;
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Domain.Common;
using PaymentOrchestrator.Domain.Payments;

namespace PaymentOrchestrator.Application.Payments.Commands.CapturePayment;

public class CapturePaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    IClock clock)
    : IRequestHandler<CapturePaymentCommand, Unit>
{
    public async Task<Unit> Handle(CapturePaymentCommand request, CancellationToken cancellationToken)
    {
        var paymentId = new PaymentId(request.PaymentId);
        var payment = await paymentRepository.GetByIdAsync(paymentId, cancellationToken) ?? throw new DomainException($"Payment {request.PaymentId} not found.");
        payment.MarkCapture(request.Amount, clock.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}