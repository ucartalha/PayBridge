using Microsoft.EntityFrameworkCore;
using PayBridge.Modules.Merchants.Domain.Merchants.Entities;

namespace PayBridge.Modules.Merchants.Infrastructure.Persistence
{
    internal sealed class MerchantsDbContext: DbContext
    {
        public MerchantsDbContext(DbContextOptions<MerchantsDbContext> options): base(options)
        {
        }

        public DbSet<Merchant> Merchants => Set<Merchant>();
        public DbSet<MerchantSector> MerchantSectors => Set<MerchantSector>();
        public DbSet<MerchantCategoryCode> MerchantCategoryCodes => Set<MerchantCategoryCode>();
        public DbSet<MerchantPaymentChannelSetting> MerchantPaymentChannelSettings => Set<MerchantPaymentChannelSetting>();
        public DbSet<MerchantProviderAccount> MerchantProviderAccounts => Set<MerchantProviderAccount>();
        public DbSet<MerchantProviderCredential> MerchantProviderCredentials => Set<MerchantProviderCredential>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MerchantsDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
