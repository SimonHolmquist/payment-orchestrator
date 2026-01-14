using MediatR;
using PaymentOrchestrator.Application.Common.Exceptions;
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Domain.Payments;

namespace PaymentOrchestrator.Application.Webhooks.Commands.ProcessPspWebhook;

public sealed class ProcessPspWebhookCommandHandler(
    IPaymentRepository repo,
    IUnitOfWork uow,
    IInboxStore inbox,
    IClock clock)
    : IRequestHandler<ProcessPspWebhookCommand>
{
    public async Task<Unit> Handle(ProcessPspWebhookCommand request, CancellationToken ct)
    {
        var now = clock.UtcNow;

        var shouldProcess = await inbox.TryBeginProcessAsync(
            provider: request.Provider,
            providerEventId: request.ProviderEventId,
            eventType: request.EventType,
            rawPayload: request.RawPayload,
            receivedAt: now,
            ct: ct);

        if (!shouldProcess) return Unit.Value; // duplicate: 200 OK desde API

        try
        {
            var payment = await repo.GetByIdAsync(new PaymentId(request.PaymentId), ct) ?? throw new NotFoundException($"Payment '{request.PaymentId}' not found.");

            // M3: mapping mínimo (sin signature validation aún).
            // EventType sugeridos: authorized|captured|refunded|cancelled|failed|unknown
            switch (request.EventType.Trim().ToLowerInvariant())
            {
                case "authorized":
                    payment.MarkAuthorized(pspReference: "psp-ref-from-webhook", at: now);
                    break;

                case "captured":
                    // En webhook real vendría amount; M3 mínimo:
                    payment.MarkCapture(amountToCapture: payment.Amount.Amount - payment.CapturedAmount, at: now);
                    break;

                case "refunded":
                    payment.MarkRefund(amountToRefund: payment.CapturedAmount - payment.RefundedAmount, at: now);
                    break;

                case "cancelled":
                    payment.Cancel(at: now);
                    break;

                case "failed":
                    payment.Fail(reason: "psp-fail", at: now);
                    break;

                default:
                    payment.MarkUnknown(reason: $"Unmapped eventType '{request.EventType}'", at: now);
                    break;
            }

            await uow.SaveChangesAsync(ct);

            await inbox.MarkProcessedAsync(request.Provider, request.ProviderEventId, now, ct);

            return Unit.Value;
        }
        catch (Exception ex)
        {
            await inbox.MarkFailedAsync(request.Provider, request.ProviderEventId, now, ex.Message, ct);
            throw;
        }
    }
}