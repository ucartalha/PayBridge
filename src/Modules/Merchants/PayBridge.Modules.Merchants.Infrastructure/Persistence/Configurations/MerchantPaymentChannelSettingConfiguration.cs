using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayBridge.Modules.Merchants.Domain.Merchants.Entities;

namespace PayBridge.Modules.Merchants.Infrastructure.Persistence.Configurations;

internal sealed class MerchantPaymentChannelSettingConfiguration
    : IEntityTypeConfiguration<MerchantPaymentChannelSetting>
{
    public void Configure(EntityTypeBuilder<MerchantPaymentChannelSetting> builder)
    {
        builder.ToTable("MerchantPaymentChannelSettings", "merchants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.MerchantId)
            .IsRequired();

        builder.Property(x => x.Channel)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.IsEnabled)
            .IsRequired();

        builder.Property(x => x.MinAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.MaxAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.DailyAmountLimit)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Require3DS)
            .IsRequired();

        builder.Property(x => x.AllowRefund)
            .IsRequired();

        builder.Property(x => x.AllowVoid)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.EnabledAtUtc);

        builder.Property(x => x.DisabledAtUtc);

        builder.HasIndex(x => new { x.MerchantId, x.Channel })
            .IsUnique();

        builder.HasIndex(x => x.IsEnabled);

        builder.HasOne<Merchant>()
            .WithMany()
            .HasForeignKey(x => x.MerchantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}