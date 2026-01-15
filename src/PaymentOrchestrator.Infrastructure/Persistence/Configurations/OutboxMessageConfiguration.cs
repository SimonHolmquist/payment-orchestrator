using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentOrchestrator.Infrastructure.Persistence.Entities;

namespace PaymentOrchestrator.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).HasMaxLength(300).IsRequired();
        builder.Property(x => x.AggregateType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(100).IsRequired();

        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.Error);

        builder.HasIndex(x => x.PublishedAt);
        builder.Property(x => x.ProcessedAt);
        builder.HasIndex(x => x.ProcessedAt)
               .HasFilter("[ProcessedAt] IS NULL");
    }
}