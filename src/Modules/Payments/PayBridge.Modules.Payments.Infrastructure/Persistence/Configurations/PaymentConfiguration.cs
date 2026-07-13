using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayBridge.Modules.Payments.Domain.Payments.Entities;

namespace PayBridge.Modules.Payments.Infrastructure.Persistence.Configurations;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments", "payments");

        builder.HasKey(payment => payment.Id);

        builder.Property(payment => payment.MerchantId)
            .IsRequired();

        builder.Property(payment => payment.OrderId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(payment => payment.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(payment => payment.RefundedAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(payment => payment.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(payment => payment.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(payment => payment.ProviderCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(payment => payment.ProviderTransactionId)
            .HasMaxLength(100);

        builder.Property(payment => payment.CreatedAtUtc)
            .IsRequired();

        builder.Property(payment => payment.UpdatedAtUtc);

        builder.Ignore(payment => payment.RefundableAmount);

        builder.HasIndex(payment => payment.MerchantId);

        builder.HasIndex(payment => new
        {
            payment.MerchantId,
            payment.OrderId
        });
    }
}