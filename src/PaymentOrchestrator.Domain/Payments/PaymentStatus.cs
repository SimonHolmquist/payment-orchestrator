namespace PaymentOrchestrator.Domain.Payments;

public enum PaymentStatus
{
    Created = 0,

    AuthorizationRequested = 10,
    CaptureRequested = 11,
    RefundRequested = 12,
    CancelRequested = 13,

    Authorized = 20,
    PartiallyCaptured = 30,
    Captured = 31,
    PartiallyRefunded = 40,
    Refunded = 41,
    Cancelled = 50,
    Failed = 60,
    Unknown = 70
}
