using MediatR;
using PaymentOrchestrator.Application.Common.Exceptions;
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Domain.Payments;

namespace PaymentOrchestrator.Application.Payments.Commands.ReconcilePayment;

public sealed class ReconcilePaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    IPspClient pspClient,
    IClock clock)
    : IRequestHandler<ReconcilePaymentCommand, Unit>
{
    public async Task<Unit> Handle(ReconcilePaymentCommand request, CancellationToken ct)
    {
        var id = new PaymentId(request.PaymentId);
        var payment = await paymentRepository.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Payment {request.PaymentId} not found.");

        // Si no hay PSP reference, lo marcamos Unknown (reconciliación no posible)
        if (string.IsNullOrWhiteSpace(payment.PspReference))
        {
            payment.MarkUnknown("Missing PSP reference for reconciliation.", clock.UtcNow);
            await unitOfWork.SaveChangesAsync(ct);
            return Unit.Value;
        }

        var snap = await pspClient.GetPaymentSnapshotAsync(payment.PspReference!, ct);

        // Map PSP -> Dominio (transiciones seguras)
        switch (snap.Status)
        {
            case PspPaymentStatus.Authorized:
                if (payment.Status is PaymentStatus.AuthorizationRequested or PaymentStatus.Unknown or PaymentStatus.Created)
                    payment.MarkAuthorized(payment.PspReference!, clock.UtcNow);
                break;

            case PspPaymentStatus.Captured:
                // Captura delta (si PSP no envía amount, asumimos total restante)
                var remainingToCapture = payment.Amount.Amount - payment.CapturedAmount;
                var targetCaptured = snap.CapturedAmount ?? payment.Amount.Amount;
                var deltaCapture = Math.Max(0m, targetCaptured - payment.CapturedAmount);

                if (deltaCapture > 0m && deltaCapture <= remainingToCapture)
                    payment.MarkCapture(deltaCapture, clock.UtcNow);
                break;

            case PspPaymentStatus.Refunded:
                var targetRefunded = snap.RefundedAmount ?? payment.Amount.Amount;
                var deltaRefund = Math.Max(0m, targetRefunded - payment.RefundedAmount);
                if (deltaRefund > 0m)
                    payment.MarkRefund(deltaRefund, clock.UtcNow);
                break;

            case PspPaymentStatus.Cancelled:
                payment.Cancel(clock.UtcNow);
                break;

            case PspPaymentStatus.Failed:
                payment.Fail(snap.FailureReason ?? "Reconciled as Failed by PSP.", clock.UtcNow);
                break;

            default:
                payment.MarkUnknown("PSP returned Unknown state.", clock.UtcNow);
                break;
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
