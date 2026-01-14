namespace PaymentOrchestrator.Application.Common.Interfaces;

public sealed record IdempotencyRecord(
    string ClientId,
    string Key,
    string OperationName,
    string RequestHash,
    DateTimeOffset CreatedAt);
