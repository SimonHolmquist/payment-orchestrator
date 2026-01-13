using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentOrchestrator.Domain.Payments;
using PaymentOrchestrator.Domain.Payments.ValueObjects;

namespace PaymentOrchestrator.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        // Conversión de PaymentId fuertemente tipado a Guid
        builder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => new PaymentId(value));

        builder.Property(p => p.ClientId)
            .HasMaxLength(100)
            .IsRequired();

        // Mapeo de Value Object Money como Owned Entity (Columns: Amount, Currency)
        builder.OwnsOne(p => p.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2); // Estándar financiero

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3);
        });

        // Configuración de concurrencia optimista
        builder.Property(p => p.Status)
            .HasConversion<string>(); // O int, según preferencia. Roadmap sugiere SQL como source of truth.

        // RowVersion para concurrencia (Opcional pero recomendado en M2)
        builder.Property<byte[]>("RowVersion")
            .IsRowVersion();
    }
}
