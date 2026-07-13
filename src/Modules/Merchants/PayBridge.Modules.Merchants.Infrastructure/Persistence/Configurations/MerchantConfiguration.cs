using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayBridge.Modules.Merchants.Domain.Merchants.Entities;

namespace PayBridge.Modules.Merchants.Infrastructure.Persistence.Configurations;

internal sealed class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
{
    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        builder.ToTable("Merchants", "merchants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.MerchantCode)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.LegalName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.TaxNumber)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.TaxOffice)
            .HasMaxLength(128);

        builder.Property(x => x.SectorId)
            .IsRequired();

        builder.Property(x => x.MccId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.ActivatedAtUtc);

        builder.Property(x => x.SuspendedAtUtc);

        builder.Property(x => x.ClosedAtUtc);

        builder.HasIndex(x => x.MerchantCode)
            .IsUnique();

        builder.HasIndex(x => x.SectorId);

        builder.HasIndex(x => x.MccId);

        builder.HasIndex(x => x.Status);

        builder.HasOne<MerchantSector>()
            .WithMany()
            .HasForeignKey(x => x.SectorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<MerchantCategoryCode>()
            .WithMany()
            .HasForeignKey(x => x.MccId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}