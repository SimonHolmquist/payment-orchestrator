using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentOrchestrator.Infrastructure.Persistence.Entities;

namespace PaymentOrchestrator.Infrastructure.Persistence.Configurations;

internal class IdempotencyKeyConfiguration : IEntityTypeConfiguration<IdempotencyKey>
{
    public void Configure(EntityTypeBuilder<IdempotencyKey> builder)
    {
        builder.ToTable("IdempotencyKeys");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClientId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Key).HasMaxLength(200).IsRequired();
        builder.Property(x => x.OperationName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.RequestHash).HasMaxLength(64).IsRequired();

        builder.Property(x => x.ResponseContentType).HasMaxLength(100);

        builder.HasIndex(x => new { x.ClientId, x.Key }).IsUnique();
    }
}
