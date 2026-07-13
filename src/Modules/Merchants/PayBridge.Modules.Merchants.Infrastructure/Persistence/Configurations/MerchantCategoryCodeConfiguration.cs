using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayBridge.Modules.Merchants.Domain.Merchants.Entities;

namespace PayBridge.Modules.Merchants.Infrastructure.Persistence.Configurations;

internal sealed class MerchantCategoryCodeConfiguration : IEntityTypeConfiguration<MerchantCategoryCode>
{
    public void Configure(EntityTypeBuilder<MerchantCategoryCode> builder)
    {
        builder.ToTable("MerchantCategoryCodes", "merchants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(16);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.IsRestricted)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.DeactivatedAtUtc);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.HasIndex(x => x.IsActive);

        builder.HasIndex(x => x.IsRestricted);
    }
}