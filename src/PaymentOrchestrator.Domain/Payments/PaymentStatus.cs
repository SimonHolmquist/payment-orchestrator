namespace PaymentOrchestrator.Domain.Payments;

public enum PaymentStatus
{
    Created = 0,
    Authorized = 1,
    PartiallyCaptured = 2,
    Captured = 3,
    PartiallyRefunded = 4,
    Refunded = 5,
    Cancelled = 6,
    Failed = 7,
    Unknown = 8
}
