namespace PaymentOrchestrator.Api.Contracts.Payments;

/// <summary>
/// Request to initiate a new payment process.
/// </summary>
/// <param name="ClientId">Unique identifier of the merchant/client.</param>
/// <param name="Amount">Monetary value (positive).</param>
/// <param name="Currency">3-letter ISO currency code (e.g. USD, EUR).</param>
public sealed record CreatePaymentRequest(
    string ClientId,
    decimal Amount,
    string Currency);