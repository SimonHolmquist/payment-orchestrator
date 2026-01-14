using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Domain.Payments;
using PaymentOrchestrator.Domain.Payments.ValueObjects;
using PaymentOrchestrator.Infrastructure.Persistence.Entities;

namespace PaymentOrchestrator.Infrastructure.Persistence;

public sealed class PaymentOrchestratorDbContext(DbContextOptions<PaymentOrchestratorDbContext> options)
    : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(b =>
        {
            b.ToTable("Payments");

            b.HasKey(x => x.Id);

            b.Property(x => x.Id)
                .HasConversion(
                    id => id.Value,
                    value => new PaymentId(value))
                .ValueGeneratedNever();

            b.Property(x => x.ClientId).HasMaxLength(100).IsRequired();
            b.Property(x => x.Status).HasConversion<int>().IsRequired();

            b.OwnsOne(x => x.Amount, money =>
            {
                money.Property(m => m.Amount).HasColumnName("Amount").HasColumnType("decimal(18,2)");
                money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
            });

            b.Property(x => x.CapturedAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.RefundedAmount).HasColumnType("decimal(18,2)");

            b.Property<byte[]>("RowVersion")
                .IsRowVersion()
                .HasColumnName("RowVersion");

            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.AuthorizedAt);
            b.Property(x => x.CapturedAt);
            b.Property(x => x.CancelledAt);
            b.Property(x => x.FailedAt);

            b.Property(x => x.PspReference).HasMaxLength(200);
            b.Property(x => x.FailureReason).HasMaxLength(500);
        });

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("OutboxMessages");
            b.HasKey(x => x.Id);

            b.Property(x => x.Type).HasMaxLength(300).IsRequired();
            b.Property(x => x.AggregateType).HasMaxLength(200).IsRequired();
            b.Property(x => x.CorrelationId).HasMaxLength(100).IsRequired();

            b.Property(x => x.Content).IsRequired();
            b.Property(x => x.Error);

            b.HasIndex(x => x.PublishedAt);
        });

        modelBuilder.Entity<InboxMessage>(b =>
        {
            b.ToTable("InboxMessages");
            b.HasKey(x => x.Id);

            b.Property(x => x.Provider).HasMaxLength(50).IsRequired();
            b.Property(x => x.ProviderEventId).HasMaxLength(200).IsRequired();
            b.Property(x => x.EventType).HasMaxLength(100).IsRequired();

            b.Property(x => x.RawPayload).IsRequired();

            b.HasIndex(x => new { x.Provider, x.ProviderEventId }).IsUnique();
        });

        modelBuilder.Entity<IdempotencyKey>(b =>
        {
            b.ToTable("IdempotencyKeys");
            b.HasKey(x => x.Id);

            b.Property(x => x.ClientId).HasMaxLength(100).IsRequired();
            b.Property(x => x.Key).HasMaxLength(200).IsRequired();
            b.Property(x => x.OperationName).HasMaxLength(200).IsRequired();
            b.Property(x => x.RequestHash).HasMaxLength(64).IsRequired();

            b.Property(x => x.ResponseContentType).HasMaxLength(100);

            b.HasIndex(x => new { x.ClientId, x.Key, x.OperationName }).IsUnique();
        });
    }
}
