namespace PaymentOrchestrator.Application.Common.Interfaces;

public interface IOutboxWriter
{
    Task EnqueueAsync(
        string type,
        string contentJson,
        Guid aggregateId,
        string aggregateType,
        DateTimeOffset occurredAt,
        string correlationId,
        CancellationToken ct);
}
