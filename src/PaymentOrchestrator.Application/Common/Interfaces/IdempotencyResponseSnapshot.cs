namespace PaymentOrchestrator.Application.Common.Interfaces;

public sealed record IdempotencyResponseSnapshot(
    int StatusCode,
    string ContentType,
    string Body);
