using PaymentOrchestrator.Domain.Common;
using PaymentOrchestrator.Domain.Payments.Events;
using PaymentOrchestrator.Domain.Payments.ValueObjects;

namespace PaymentOrchestrator.Domain.Payments;

public sealed class Payment
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public PaymentId Id { get; private set; }
    public string ClientId { get; private set; } = default!;
    public Money Amount { get; private set; }

    // Captures/Refunds parciales: acumuladores
    public decimal CapturedAmount { get; private set; }
    public decimal RefundedAmount { get; private set; }

    public PaymentStatus Status { get; private set; }

    public string? PspReference { get; private set; }
    public string? FailureReason { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? AuthorizedAt { get; private set; }
    public DateTimeOffset? CapturedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public DateTimeOffset? FailedAt { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Payment() { } // para ORMs

    public static Payment Create(PaymentId id, string clientId, Money amount, DateTimeOffset createdAt)
    {
        _ = Guard.NotNullOrWhiteSpace(clientId, nameof(clientId));
        if (amount.Amount <= 0) throw new DomainException("Payment amount must be > 0.");

        var payment = new Payment
        {
            Id = id,
            ClientId = clientId.Trim(),
            Amount = amount,
            CreatedAt = createdAt,
            Status = PaymentStatus.Created,
            CapturedAmount = 0,
            RefundedAmount = 0
        };

        payment.AddEvent(new PaymentCreated(payment.Id, payment.ClientId, payment.Amount, payment.CreatedAt));
        return payment;
    }

    public void MarkAuthorized(string pspReference, DateTimeOffset at)
    {
        EnsureNotFinal();
        EnsureStatus(PaymentStatus.Created, PaymentStatus.Unknown);

        PspReference = Guard.NotNullOrWhiteSpace(pspReference, nameof(pspReference));
        AuthorizedAt = at;
        Status = PaymentStatus.Authorized;

        AddEvent(new PaymentAuthorized(Id, PspReference, at));
    }

    public void MarkCapture(decimal amountToCapture, DateTimeOffset at)
    {
        EnsureNotFinal();

        if (Status is not (PaymentStatus.Authorized or PaymentStatus.PartiallyCaptured or PaymentStatus.Unknown))
            throw new DomainException($"Cannot capture a payment in status '{Status}'.");

        Guard.Positive(amountToCapture, nameof(amountToCapture));

        var remaining = Amount.Amount - CapturedAmount;
        if (amountToCapture > remaining)
            throw new DomainException("Capture amount exceeds remaining amount.");

        CapturedAmount += amountToCapture;
        CapturedAt = at;

        Status = CapturedAmount == Amount.Amount
            ? PaymentStatus.Captured
            : PaymentStatus.PartiallyCaptured;

        AddEvent(new PaymentCaptured(Id, amountToCapture, CapturedAmount, Status, at));
    }

    public void MarkRefund(decimal amountToRefund, DateTimeOffset at)
    {
        EnsureNotFinal();

        if (Status is not (PaymentStatus.Captured or PaymentStatus.PartiallyRefunded or PaymentStatus.PartiallyCaptured))
            throw new DomainException($"Cannot refund a payment in status '{Status}'.");

        Guard.Positive(amountToRefund, nameof(amountToRefund));

        var refundable = CapturedAmount - RefundedAmount;
        if (refundable <= 0)
            throw new DomainException("Nothing to refund.");

        if (amountToRefund > refundable)
            throw new DomainException("Refund amount exceeds refundable amount.");

        RefundedAmount += amountToRefund;

        Status = RefundedAmount == CapturedAmount
            ? PaymentStatus.Refunded
            : PaymentStatus.PartiallyRefunded;

        AddEvent(new PaymentRefunded(Id, amountToRefund, RefundedAmount, Status, at));
    }

    public void Cancel(DateTimeOffset at)
    {
        EnsureNotFinal();

        // Cancel sólo antes de que exista captura
        if (CapturedAmount > 0)
            throw new DomainException("Cannot cancel a payment that has captures.");

        if (Status is not (PaymentStatus.Created or PaymentStatus.Authorized or PaymentStatus.Unknown))
            throw new DomainException($"Cannot cancel a payment in status '{Status}'.");

        Status = PaymentStatus.Cancelled;
        CancelledAt = at;

        AddEvent(new PaymentCancelled(Id, at));
    }

    public void Fail(string reason, DateTimeOffset at)
    {
        EnsureNotFinal();

        FailureReason = Guard.NotNullOrWhiteSpace(reason, nameof(reason));
        Status = PaymentStatus.Failed;
        FailedAt = at;

        AddEvent(new PaymentFailed(Id, FailureReason, at));
    }

    public void MarkUnknown(string reason, DateTimeOffset at)
    {
        EnsureNotFinal();

        FailureReason = Guard.NotNullOrWhiteSpace(reason, nameof(reason));
        Status = PaymentStatus.Unknown;

        AddEvent(new PaymentMarkedUnknown(Id, FailureReason, at));
    }

    public IDomainEvent[] DequeueDomainEvents()
    {
        var copy = _domainEvents.ToArray();
        _domainEvents.Clear();
        return copy;
    }

    private void AddEvent(IDomainEvent ev) => _domainEvents.Add(ev);

    private void EnsureNotFinal()
    {
        if (Status is PaymentStatus.Cancelled or PaymentStatus.Failed or PaymentStatus.Refunded)
            throw new DomainException($"Payment is final in status '{Status}'.");
    }

    private void EnsureStatus(params PaymentStatus[] allowed)
    {
        if (!allowed.Contains(Status))
            throw new DomainException($"Invalid status transition from '{Status}'.");
    }
}
