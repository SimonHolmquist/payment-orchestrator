namespace PaymentOrchestrator.Infrastructure.Persistence.Entities;

public class IdempotencyKey
{
    public string Key { get; set; } = default!;
    public string ClientId { get; set; } = default!;
    public string OperationName { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
}