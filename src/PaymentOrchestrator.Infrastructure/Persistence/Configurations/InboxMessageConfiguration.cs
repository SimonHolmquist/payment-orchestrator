using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentOrchestrator.Infrastructure.Persistence.Entities;

namespace PaymentOrchestrator.Infrastructure.Persistence.Configurations;

public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ProviderEventId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(100).IsRequired();

        builder.Property(x => x.RawPayload).IsRequired();

        builder.HasIndex(x => new { x.Provider, x.ProviderEventId }).IsUnique();
    }
}
