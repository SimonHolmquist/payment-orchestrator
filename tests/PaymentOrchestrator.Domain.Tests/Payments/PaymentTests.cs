using PaymentOrchestrator.Domain.Common;
using PaymentOrchestrator.Domain.Payments;
using PaymentOrchestrator.Domain.Payments.Events;
using PaymentOrchestrator.Domain.Payments.ValueObjects;

namespace PaymentOrchestrator.Domain.Tests.Payments;

public sealed class PaymentTests
{
    private static readonly DateTimeOffset Now = new(2026, 01, 07, 12, 00, 00, TimeSpan.Zero);

    [Fact]
    public void Create_SetsStatusAndRaisesEvent()
    {
        var payment = Payment.Create(PaymentId.New(), "client-1", Money.Of(100, "USD"), Now);

        Assert.Equal(PaymentStatus.Created, payment.Status);
        Assert.Single(payment.DomainEvents);
        Assert.IsType<PaymentCreated>(payment.DomainEvents.First());
    }

    [Fact]
    public void Authorize_FromCreated_Transitions()
    {
        var payment = Payment.Create(PaymentId.New(), "client-1", Money.Of(100, "USD"), Now);

        payment.MarkAuthorized("psp-ref-1", Now.AddMinutes(1));

        Assert.Equal(PaymentStatus.Authorized, payment.Status);
        Assert.Equal("psp-ref-1", payment.PspReference);
        Assert.Contains(payment.DomainEvents, e => e is PaymentAuthorized);
    }

    [Fact]
    public void Authorize_Twice_Throws()
    {
        var payment = Payment.Create(PaymentId.New(), "client-1", Money.Of(100, "USD"), Now);
        payment.MarkAuthorized("psp-ref-1", Now.AddMinutes(1));

        var ex = Assert.Throws<DomainException>(() => payment.MarkAuthorized("psp-ref-2", Now.AddMinutes(2)));
        Assert.Contains("Invalid status transition", ex.Message);
    }

    [Fact]
    public void Capture_BeforeAuthorize_Throws()
    {
        var payment = Payment.Create(PaymentId.New(), "client-1", Money.Of(100, "USD"), Now);

        var ex = Assert.Throws<DomainException>(() => payment.MarkCapture(10, Now.AddMinutes(1)));
        Assert.Contains("Cannot capture", ex.Message);
    }

    [Fact]
    public void Capture_ExceedsRemaining_Throws()
    {
        var payment = Payment.Create(PaymentId.New(), "client-1", Money.Of(100, "USD"), Now);
        payment.MarkAuthorized("psp-ref-1", Now.AddMinutes(1));

        var ex = Assert.Throws<DomainException>(() => payment.MarkCapture(150, Now.AddMinutes(2)));
        Assert.Contains("exceeds remaining", ex.Message);
    }

    [Fact]
    public void Capture_PartialThenFull_TransitionsToCaptured()
    {
        var payment = Payment.Create(PaymentId.New(), "client-1", Money.Of(100, "USD"), Now);
        payment.MarkAuthorized("psp-ref-1", Now.AddMinutes(1));

        payment.MarkCapture(40, Now.AddMinutes(2));
        Assert.Equal(PaymentStatus.PartiallyCaptured, payment.Status);
        Assert.Equal(40, payment.CapturedAmount);

        payment.MarkCapture(60, Now.AddMinutes(3));
        Assert.Equal(PaymentStatus.Captured, payment.Status);
        Assert.Equal(100, payment.CapturedAmount);
    }

    [Fact]
    public void Refund_BeforeCapture_Throws()
    {
        var payment = Payment.Create(PaymentId.New(), "client-1", Money.Of(100, "USD"), Now);
        payment.MarkAuthorized("psp-ref-1", Now.AddMinutes(1));

        var ex = Assert.Throws<DomainException>(() => payment.MarkRefund(10, Now.AddMinutes(2)));
        Assert.Contains("Cannot refund", ex.Message);
    }

    [Fact]
    public void Refund_ExceedsRefundable_Throws()
    {
        var payment = Payment.Create(PaymentId.New(), "client-1", Money.Of(100, "USD"), Now);
        payment.MarkAuthorized("psp-ref-1", Now.AddMinutes(1));
        payment.MarkCapture(100, Now.AddMinutes(2));

        var ex = Assert.Throws<DomainException>(() => payment.MarkRefund(120, Now.AddMinutes(3)));
        Assert.Contains("exceeds refundable", ex.Message);
    }

    [Fact]
    public void Refund_PartialThenFull_TransitionsToRefunded()
    {
        var payment = Payment.Create(PaymentId.New(), "client-1", Money.Of(100, "USD"), Now);
        payment.MarkAuthorized("psp-ref-1", Now.AddMinutes(1));
        payment.MarkCapture(100, Now.AddMinutes(2));

        payment.MarkRefund(30, Now.AddMinutes(3));
        Assert.Equal(PaymentStatus.PartiallyRefunded, payment.Status);
        Assert.Equal(30, payment.RefundedAmount);

        payment.MarkRefund(70, Now.AddMinutes(4));
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        Assert.Equal(100, payment.RefundedAmount);
    }

    [Fact]
    public void Cancel_FromCreated_Works()
    {
        var payment = Payment.Create(PaymentId.New(), "client-1", Money.Of(100, "USD"), Now);

        payment.Cancel(Now.AddMinutes(1));

        Assert.Equal(PaymentStatus.Cancelled, payment.Status);
        Assert.Contains(payment.DomainEvents, e => e is PaymentCancelled);
    }

    [Fact]
    public void Cancel_AfterCapture_Throws()
    {
        var payment = Payment.Create(PaymentId.New(), "client-1", Money.Of(100, "USD"), Now);
        payment.MarkAuthorized("psp-ref-1", Now.AddMinutes(1));
        payment.MarkCapture(100, Now.AddMinutes(2));

        var ex = Assert.Throws<DomainException>(() => payment.Cancel(Now.AddMinutes(3)));
        Assert.Contains("Cannot cancel", ex.Message);
    }

    [Fact]
    public void Fail_MakesPaymentFinal()
    {
        var payment = Payment.Create(PaymentId.New(), "client-1", Money.Of(100, "USD"), Now);

        payment.Fail("psp_down", Now.AddMinutes(1));

        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Throws<DomainException>(() => payment.MarkAuthorized("psp-ref-1", Now.AddMinutes(2)));
    }
}
