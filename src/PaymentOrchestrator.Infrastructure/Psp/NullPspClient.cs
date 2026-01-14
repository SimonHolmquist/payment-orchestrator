using PaymentOrchestrator.Application.Common.Interfaces;

namespace PaymentOrchestrator.Infrastructure.Psp;

public sealed class NullPspClient : IPspClient
{
    public Task<bool> AuthorizeAsync(string paymentId, decimal amount, string currency, CancellationToken ct)
        => Task.FromResult(true);

    public Task<PspPaymentSnapshot> GetPaymentSnapshotAsync(string pspReference, CancellationToken ct)
        => Task.FromResult(new PspPaymentSnapshot(PspPaymentStatus.Unknown));
}
