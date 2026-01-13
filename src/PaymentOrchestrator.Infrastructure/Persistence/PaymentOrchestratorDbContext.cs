using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Domain.Payments;
using PaymentOrchestrator.Infrastructure.Persistence.Entities;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace PaymentOrchestrator.Infrastructure.Persistence;

public class PaymentOrchestratorDbContext(DbContextOptions<PaymentOrchestratorDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}