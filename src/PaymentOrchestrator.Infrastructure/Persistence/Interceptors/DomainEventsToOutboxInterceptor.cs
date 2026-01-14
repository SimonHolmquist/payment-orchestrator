using Microsoft.EntityFrameworkCore.Diagnostics;
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Domain.Common;
using PaymentOrchestrator.Domain.Payments;
using PaymentOrchestrator.Infrastructure.Persistence.Entities;
using PaymentOrchestrator.Infrastructure.Serialization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization; // <--- NECESARIO

namespace PaymentOrchestrator.Infrastructure.Persistence.Interceptors;

public sealed class DomainEventsToOutboxInterceptor(ICorrelationContext correlationContext)
    : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        var payments = context.ChangeTracker.Entries<Payment>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .ToArray();

        foreach (var entry in payments)
        {
            var payment = entry.Entity;
            var events = payment.DequeueDomainEvents();

            foreach (var domainEvent in events)
            {
                var eventType = domainEvent.GetType();
                var eventName = eventType.GetCustomAttribute<EventNameAttribute>()?.Name ?? eventType.Name;
                // 1. Generar Outbox Message
                var outboxMsg = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = eventName,
                    Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonConfig.Default),
                    AggregateId = payment.Id.Value,
                    AggregateType = nameof(Payment),
                    CorrelationId = correlationContext.CorrelationId,
                    OccurredAt = domainEvent.OccurredAt,
                    Attempts = 0
                };
                context.Set<OutboxMessage>().Add(outboxMsg);

                // 2. Generar Ledger Entry (Auditoría)
                var ledgerEntry = new PaymentLedgerEntry
                {
                    Id = Guid.NewGuid(),
                    PaymentId = payment.Id.Value,
                    Action = eventName,
                    OccurredAt = domainEvent.OccurredAt,
                    CorrelationId = correlationContext.CorrelationId,
                    // APLICAR LAS OPCIONES AQUÍ:
                    StateSnapshot = JsonSerializer.Serialize(new
                    {
                        payment.Status,
                        payment.Amount,
                        payment.CapturedAmount,
                        payment.RefundedAmount,
                        payment.PspReference,
                        payment.FailureReason
                    }, JsonConfig.Default)
                };
                context.Set<PaymentLedgerEntry>().Add(ledgerEntry);
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}