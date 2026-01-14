using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Domain.Payments;
using PaymentOrchestrator.Infrastructure.Persistence.Entities;
using System.Text.Json;

namespace PaymentOrchestrator.Infrastructure.Persistence;

public sealed class EfUnitOfWork(
    PaymentOrchestratorDbContext db,
    ICorrelationContext correlation)
    : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        EnqueueDomainEventsToOutbox();
        await db.SaveChangesAsync(ct);
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct = default)
    {
        // EF Core uses implicit transaction for SaveChanges, pero acá garantizamos transacción alrededor de múltiples SaveChanges si existieran.
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            var result = await action(ct);
            await tx.CommitAsync(ct);
            return result;
        });
    }

    private void EnqueueDomainEventsToOutbox()
    {
        var payments = db.ChangeTracker.Entries<Payment>()
            .Select(e => e.Entity)
            .ToArray();

        foreach (var payment in payments)
        {
            var events = payment.DequeueDomainEvents();
            foreach (var ev in events)
            {
                var msg = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = ev.GetType().FullName ?? ev.GetType().Name,
                    Content = JsonSerializer.Serialize(ev, ev.GetType()),
                    AggregateId = payment.Id.Value,
                    AggregateType = nameof(Payment),
                    CorrelationId = correlation.CorrelationId,
                    OccurredAt = ExtractOccurredAt(ev)
                };

                db.OutboxMessages.Add(msg);
            }
        }
    }

    private static DateTimeOffset ExtractOccurredAt(object ev)
    {
        // Convención: events tienen propiedad OccurredAt
        var prop = ev.GetType().GetProperty("OccurredAt");
        if (prop?.PropertyType == typeof(DateTimeOffset))
            return (DateTimeOffset)prop.GetValue(ev)!;

        return DateTimeOffset.UtcNow;
    }
}
