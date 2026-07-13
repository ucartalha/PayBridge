using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayBridge.Modules.Merchants.Domain.Merchants.Entities;

namespace PayBridge.Modules.Merchants.Infrastructure.Persistence.Configurations;

internal sealed class MerchantProviderAccountConfiguration
    : IEntityTypeConfiguration<MerchantProviderAccount>
{
    public void Configure(EntityTypeBuilder<MerchantProviderAccount> builder)
    {
        builder.ToTable("MerchantProviderAccounts", "merchants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.MerchantId)
            .IsRequired();

        builder.Property(x => x.ProviderCode)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.AllowECommerce)
            .IsRequired();

        builder.Property(x => x.AllowPhysicalPos)
            .IsRequired();

        builder.Property(x => x.AllowRefund)
            .IsRequired();

        builder.Property(x => x.Priority)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.ActivatedAtUtc);

        builder.Property(x => x.DeactivatedAtUtc);

        builder.HasIndex(x => new { x.MerchantId, x.ProviderCode })
            .IsUnique();

        builder.HasIndex(x => x.ProviderCode);

        builder.HasIndex(x => x.IsActive);

        builder.HasIndex(x => x.Priority);

        builder.HasOne<Merchant>()
            .WithMany()
            .HasForeignKey(x => x.MerchantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}