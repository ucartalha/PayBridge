using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayBridge.Modules.Payments.Domain.Payments.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Payments.Infrastructure.Persistence.Configurations
{
    public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.ToTable("PaymentTransactions", "payments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PaymentId)
                .IsRequired();

            builder.Property(x => x.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.ProviderTransactionId)
                .HasMaxLength(100);

            builder.Property(x => x.ErrorCode)
                .HasMaxLength(50);

            builder.Property(x => x.ErrorMessage)
                .HasMaxLength(500);

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            // Payment -> PaymentTransactions İlişkisi (One-to-Many)
            builder.HasOne<Payment>()
                .WithMany() // Eğer Payment entity içinde IReadOnlyCollection<PaymentTransaction> yoksa boş bırakılır, varsa x => x.Transactions yazılır
                .HasForeignKey(x => x.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index'ler
            builder.HasIndex(x => x.PaymentId);
        }
    }
}
