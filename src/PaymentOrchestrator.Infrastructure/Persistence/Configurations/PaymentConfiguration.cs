using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentOrchestrator.Domain.Payments;

namespace PaymentOrchestrator.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new PaymentId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.ClientId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();

        builder.OwnsOne(x => x.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Amount").HasColumnType("decimal(18,2)");
            money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        builder.Property(x => x.CapturedAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.RefundedAmount).HasColumnType("decimal(18,2)");

        // Configuración de RowVersion
        builder.Property<byte[]>("RowVersion")
            .IsRowVersion()
            .HasColumnName("RowVersion");

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.AuthorizedAt);
        builder.Property(x => x.CapturedAt);
        builder.Property(x => x.CancelledAt);
        builder.Property(x => x.FailedAt);

        builder.Property(x => x.PspReference).HasMaxLength(200);
        builder.Property(x => x.FailureReason).HasMaxLength(500);
    }
}