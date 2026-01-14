using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentOrchestrator.Infrastructure.Persistence.Entities;

namespace PaymentOrchestrator.Infrastructure.Persistence.Configurations;

public class PaymentLedgerEntryConfiguration : IEntityTypeConfiguration<PaymentLedgerEntry>
{
    public void Configure(EntityTypeBuilder<PaymentLedgerEntry> builder)
    {
        builder.ToTable("PaymentLedger");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).IsRequired();
        builder.Property(x => x.OccurredAt).IsRequired();

        // Índice para búsquedas rápidas por pago
        builder.HasIndex(x => x.PaymentId);
    }
}