using PaymentOrchestrator.Domain.Common;
using PaymentOrchestrator.Domain.Payments;
using PaymentOrchestrator.Domain.Payments.Events;
using PaymentOrchestrator.Domain.Tests.Utils; // Importante: usar el namespace del Builder

namespace PaymentOrchestrator.Domain.Tests.Payments;

public sealed class PaymentTests
{
    private static readonly DateTimeOffset Now = new(2026, 01, 07, 12, 00, 00, TimeSpan.Zero);

    [Fact]
    public void Create_SetsStatusAndRaisesEvent()
    {
        // Arrange & Act
        var payment = new PaymentBuilder()
            .WithAmount(100, "USD")
            .WithClientId("client-1")
            .Build();

        // Assert
        Assert.Equal(PaymentStatus.Created, payment.Status);
        Assert.Single(payment.DomainEvents);
        Assert.IsType<PaymentCreated>(payment.DomainEvents.First());
    }

    [Fact]
    public void Authorize_FromCreated_Transitions()
    {
        var payment = new PaymentBuilder().Build(); // Estado por defecto: Created

        payment.MarkAuthorized("psp-ref-1", Now);

        Assert.Equal(PaymentStatus.Authorized, payment.Status);
        Assert.Equal("psp-ref-1", payment.PspReference);
        Assert.Contains(payment.DomainEvents, e => e is PaymentAuthorized);
    }

    [Fact]
    public void Authorize_Twice_Throws()
    {
        // Arrange: Empezamos directamente con un pago Autorizado
        var payment = new PaymentBuilder()
            .Authorized()
            .Build();

        // Act & Assert
        var ex = Assert.Throws<DomainException>(() => payment.MarkAuthorized("psp-ref-2", Now));
        Assert.Contains("Invalid status transition", ex.Message);
    }

    [Fact]
    public void Capture_BeforeAuthorize_Throws()
    {
        var payment = new PaymentBuilder().Build(); // Created

        var ex = Assert.Throws<DomainException>(() => payment.MarkCapture(10, Now));
        Assert.Contains("Cannot capture", ex.Message);
    }

    [Fact]
    public void Capture_ExceedsRemaining_Throws()
    {
        // Arrange: Pago de 100 USD ya Autorizado
        var payment = new PaymentBuilder()
            .WithAmount(100)
            .Authorized()
            .Build();

        // Act & Assert
        var ex = Assert.Throws<DomainException>(() => payment.MarkCapture(150, Now));
        Assert.Contains("exceeds remaining", ex.Message);
    }

    [Fact]
    public void Capture_PartialThenFull_TransitionsToCaptured()
    {
        var payment = new PaymentBuilder()
            .WithAmount(100)
            .Authorized()
            .Build();

        payment.MarkCapture(40, Now);
        Assert.Equal(PaymentStatus.PartiallyCaptured, payment.Status);
        Assert.Equal(40, payment.CapturedAmount);

        payment.MarkCapture(60, Now.AddMinutes(1));
        Assert.Equal(PaymentStatus.Captured, payment.Status);
        Assert.Equal(100, payment.CapturedAmount);
    }

    [Fact]
    public void Refund_BeforeCapture_Throws()
    {
        // Arrange: Autorizado pero no capturado
        var payment = new PaymentBuilder().Authorized().Build();

        var ex = Assert.Throws<DomainException>(() => payment.MarkRefund(10, Now));
        Assert.Contains("Cannot refund", ex.Message);
    }

    [Fact]
    public void Refund_ExceedsRefundable_Throws()
    {
        // Arrange: Pago de 100, Capturado totalmente (100)
        var payment = new PaymentBuilder()
            .WithAmount(100)
            .Captured(100) // <--- El builder maneja la transición interna
            .Build();

        // Act & Assert
        var ex = Assert.Throws<DomainException>(() => payment.MarkRefund(120, Now));
        Assert.Contains("exceeds refundable", ex.Message);
    }

    [Fact]
    public void Refund_PartialThenFull_TransitionsToRefunded()
    {
        // Arrange: Pago de 100, Capturado totalmente
        var payment = new PaymentBuilder()
            .WithAmount(100)
            .Captured(100)
            .Build();

        // Act
        payment.MarkRefund(30, Now);
        Assert.Equal(PaymentStatus.PartiallyRefunded, payment.Status);
        Assert.Equal(30, payment.RefundedAmount);

        payment.MarkRefund(70, Now.AddMinutes(1));
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        Assert.Equal(100, payment.RefundedAmount);
    }

    [Fact]
    public void Cancel_FromCreated_Works()
    {
        var payment = new PaymentBuilder().Build();

        payment.Cancel(Now);

        Assert.Equal(PaymentStatus.Cancelled, payment.Status);
        Assert.Contains(payment.DomainEvents, e => e is PaymentCancelled);
    }

    [Fact]
    public void Cancel_AfterCapture_Throws()
    {
        // Arrange: Pago capturado
        var payment = new PaymentBuilder()
            .WithAmount(100)
            .Captured(100)
            .Build();

        // Act & Assert
        var ex = Assert.Throws<DomainException>(() => payment.Cancel(Now));
        Assert.Contains("Cannot cancel", ex.Message);
    }

    [Fact]
    public void Fail_MakesPaymentFinal()
    {
        var payment = new PaymentBuilder().Build();

        payment.Fail("psp_down", Now);

        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Throws<DomainException>(() => payment.MarkAuthorized("psp-ref-1", Now));
    }
}