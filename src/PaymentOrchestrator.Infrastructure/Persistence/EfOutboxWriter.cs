using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Infrastructure.Persistence.Entities;

namespace PaymentOrchestrator.Infrastructure.Persistence;

public sealed class EfOutboxWriter(PaymentOrchestratorDbContext db) : IOutboxWriter
{
    public Task EnqueueAsync(
        string type,
        string contentJson,
        Guid aggregateId,
        string aggregateType,
        DateTimeOffset occurredAt,
        string correlationId,
        CancellationToken ct)
    {
        db.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = type,
            Content = contentJson,
            AggregateId = aggregateId,
            AggregateType = aggregateType,
            OccurredAt = occurredAt,
            CorrelationId = correlationId,
            Attempts = 0
        });

        return Task.CompletedTask;
    }
}
