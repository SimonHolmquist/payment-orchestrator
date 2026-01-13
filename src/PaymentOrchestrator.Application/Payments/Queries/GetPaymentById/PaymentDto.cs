namespace PaymentOrchestrator.Application.Payments.Queries.GetPaymentById;

public record PaymentDto(
    Guid Id,
    string ClientId,
    decimal Amount,
    string Currency,
    string Status,
    string? PspReference,
    string? FailureReason,
    DateTimeOffset CreatedAt
);
