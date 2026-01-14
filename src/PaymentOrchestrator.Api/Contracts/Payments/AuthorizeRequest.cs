namespace PaymentOrchestrator.Api.Contracts.Payments;

public sealed record AuthorizeRequest(string PspReference);