using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PaymentOrchestrator.Domain.Payments;
using PaymentOrchestrator.Infrastructure.Persistence.Entities;
using System.Text.Json;

namespace PaymentOrchestrator.Infrastructure.Persistence.Interceptors;

public class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ConvertDomainEventsToOutboxMessages(eventData.Context).GetAwaiter().GetResult();
        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        await ConvertDomainEventsToOutboxMessages(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static async Task ConvertDomainEventsToOutboxMessages(DbContext? context)
    {
        if (context is null) return;

        var aggregates = context.ChangeTracker
            .Entries<Payment>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregates
            .SelectMany(a => a.DequeueDomainEvents())
            .ToList();

        if (domainEvents.Count == 0) return;

        var outboxMessages = domainEvents.Select(domainEvent => new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.UtcNow,
            Type = domainEvent.GetType().Name,
            Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType())
        }).ToList();

        await context.Set<OutboxMessage>().AddRangeAsync(outboxMessages);
    }
}