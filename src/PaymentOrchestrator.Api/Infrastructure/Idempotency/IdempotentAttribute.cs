namespace PaymentOrchestrator.Api.Infrastructure.Idempotency;

[AttributeUsage(AttributeTargets.Method)]
public sealed class IdempotentAttribute : Attribute
{
    public string Operation { get; set; } = "Unknown";
}