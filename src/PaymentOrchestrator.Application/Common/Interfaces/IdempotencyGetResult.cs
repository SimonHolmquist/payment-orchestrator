namespace PaymentOrchestrator.Application.Common.Interfaces;

public sealed record IdempotencyGetResult(
    string ClientId,
    string Key,
    string OperationName,
    string RequestHash,
    IdempotencyResponseSnapshot? Response);
