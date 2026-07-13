using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayBridge.Modules.Merchants.Domain.Merchants.Entities;

namespace PayBridge.Modules.Merchants.Infrastructure.Persistence.Configurations;

internal sealed class MerchantSectorConfiguration : IEntityTypeConfiguration<MerchantSector>
{
    public void Configure(EntityTypeBuilder<MerchantSector> builder)
    {
        builder.ToTable("MerchantSectors", "merchants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.IsHighRisk)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.DeactivatedAtUtc);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.HasIndex(x => x.IsActive);
    }
}