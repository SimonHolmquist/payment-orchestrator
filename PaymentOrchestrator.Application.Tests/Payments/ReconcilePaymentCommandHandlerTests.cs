using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Application.Payments.Commands.ReconcilePayment;
using PaymentOrchestrator.Application.Tests.Fakes;
using PaymentOrchestrator.Domain.Payments;
using PaymentOrchestrator.Domain.Payments.ValueObjects;

namespace PaymentOrchestrator.Application.Tests.Payments;

public sealed class ReconcilePaymentCommandHandlerTests
{
    [Fact]
    public async Task Reconcile_Captured_SetsCapturedStatus()
    {
        var now = new DateTimeOffset(2026, 1, 14, 12, 0, 0, TimeSpan.Zero);
        var clock = new FakeClock(now);
        var uow = new FakeUnitOfWork();
        var repo = new InMemoryPaymentRepository();
        var psp = new FakePspClient(new PspPaymentSnapshot(PspPaymentStatus.Captured, CapturedAmount: 100m));

        var paymentId = PaymentId.New();
        var payment = Payment.Create(paymentId, "client", Money.Of(100m, "USD"), now.AddMinutes(-10));
        payment.MarkAuthorized("psp-123", now.AddMinutes(-9));
        await repo.AddAsync(payment, default);

        var handler = new ReconcilePaymentCommandHandler(repo, uow, psp, clock);
        await handler.Handle(new ReconcilePaymentCommand(paymentId.Value), default);

        Assert.Equal(PaymentStatus.Captured, payment.Status);
        Assert.Equal(100m, payment.CapturedAmount);
        Assert.Equal(1, uow.SaveChangesCalls);
    }

    private sealed class FakeClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow => now;
    }

    private sealed class FakePspClient(PspPaymentSnapshot snapshot) : IPspClient
    {
        public Task<bool> AuthorizeAsync(string paymentId, decimal amount, string currency, CancellationToken ct)
            => Task.FromResult(true);

        public Task<PspPaymentSnapshot> GetPaymentSnapshotAsync(string pspReference, CancellationToken ct)
            => Task.FromResult(snapshot);
    }

    private sealed class InMemoryPaymentRepository : IPaymentRepository
    {
        private readonly Dictionary<Guid, Payment> _store = [];

        public Task AddAsync(Payment payment, CancellationToken cancellationToken)
        {
            _store[payment.Id.Value] = payment;
            return Task.CompletedTask;
        }

        public Task<Payment?> GetByIdAsync(PaymentId id, CancellationToken cancellationToken)
            => Task.FromResult(_store.TryGetValue(id.Value, out var p) ? p : null);

        public Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            _store[payment.Id.Value] = payment;
            return Task.CompletedTask;
        }
    }
}
