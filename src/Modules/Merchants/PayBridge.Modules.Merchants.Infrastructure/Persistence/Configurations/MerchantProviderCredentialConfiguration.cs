using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayBridge.Modules.Merchants.Domain.Merchants.Entities;

namespace PayBridge.Modules.Merchants.Infrastructure.Persistence.Configurations;

internal sealed class MerchantProviderCredentialConfiguration
    : IEntityTypeConfiguration<MerchantProviderCredential>
{
    public void Configure(EntityTypeBuilder<MerchantProviderCredential> builder)
    {
        builder.ToTable("MerchantProviderCredentials", "merchants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.MerchantProviderAccountId)
            .IsRequired();

        builder.Property(x => x.EncryptedCredentialPayload)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.EncryptedKeyVersion)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.RotatedAtUtc);

        builder.Property(x => x.RevokedAtUtc);

        builder.HasIndex(x => x.MerchantProviderAccountId)
            .HasFilter("[IsActive] = 1")
            .IsUnique();

        builder.HasIndex(x => x.IsActive);

        builder.HasOne<MerchantProviderAccount>()
            .WithMany()
            .HasForeignKey(x => x.MerchantProviderAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}