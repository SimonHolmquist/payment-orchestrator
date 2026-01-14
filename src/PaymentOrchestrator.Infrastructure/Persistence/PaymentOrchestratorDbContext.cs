using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Domain.Payments;
using PaymentOrchestrator.Infrastructure.Persistence.Entities;

namespace PaymentOrchestrator.Infrastructure.Persistence;

public sealed class PaymentOrchestratorDbContext(DbContextOptions<PaymentOrchestratorDbContext> options)
    : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();
    public DbSet<PaymentLedgerEntry> PaymentLedger => Set<PaymentLedgerEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentOrchestratorDbContext).Assembly);
    }
}
