// src/PaymentOrchestrator.Application/Common/Interfaces/IPspClient.cs
namespace PaymentOrchestrator.Application.Common.Interfaces;

public interface IPspClient
{
    Task<bool> AuthorizeAsync(string paymentId, decimal amount, string currency, CancellationToken ct);

    // M1: requerido para reconciliación
    Task<PspPaymentSnapshot> GetPaymentSnapshotAsync(string pspReference, CancellationToken ct);
}

public sealed record PspPaymentSnapshot(
    PspPaymentStatus Status,
    decimal? CapturedAmount = null,
    decimal? RefundedAmount = null,
    string? FailureReason = null);

public enum PspPaymentStatus
{
    Authorized,
    Captured,
    Refunded,
    Cancelled,
    Failed,
    Unknown
}
