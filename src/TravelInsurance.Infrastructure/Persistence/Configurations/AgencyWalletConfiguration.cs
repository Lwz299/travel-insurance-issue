using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class AgencyWalletConfiguration : IEntityTypeConfiguration<AgencyWallet>
{
    public void Configure(EntityTypeBuilder<AgencyWallet> entity)
    {
        entity.ToTable("agency_wallets", table => table.HasCheckConstraint("CK_agency_wallets_balances", "balance >= 0 AND reserved_balance >= 0 AND reserved_balance <= balance"));
        entity.HasKey(x => x.Id);
        entity.Property(x => x.AgencyCompanyId).HasColumnName("agency_company_id");
        entity.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        entity.Property(x => x.Balance).HasColumnName("balance").HasColumnType("numeric(14,2)");
        entity.Property(x => x.ReservedBalance).HasColumnName("reserved_balance").HasColumnType("numeric(14,2)");
        entity.Property(x => x.IsActive).HasColumnName("is_active");
        entity.HasIndex(x => new { x.AgencyCompanyId, x.Currency }).IsUnique();
        entity.HasOne<Company>().WithMany().HasForeignKey(x => x.AgencyCompanyId).OnDelete(DeleteBehavior.Restrict);
        entity.HasMany(x => x.Transactions).WithOne(x => x.Wallet!).HasForeignKey(x => x.WalletId).OnDelete(DeleteBehavior.Cascade);
    }
}
