namespace PaymentOrchestrator.Api.Contracts.Payments;

public sealed record CreatePaymentRequest(
    string ClientId,
    decimal Amount,
    string Currency);