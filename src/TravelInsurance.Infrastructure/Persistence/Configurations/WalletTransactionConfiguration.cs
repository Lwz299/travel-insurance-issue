using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> entity)
    {
        entity.ToTable("wallet_transactions");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.WalletId).HasColumnName("wallet_id");
        entity.Property(x => x.PolicyId).HasColumnName("policy_id");
        entity.Property(x => x.IssuanceRequestId).HasColumnName("issuance_request_id");
        entity.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(30);
        entity.Property(x => x.Amount).HasColumnName("amount").HasColumnType("numeric(14,2)");
        entity.Property(x => x.BalanceBefore).HasColumnName("balance_before").HasColumnType("numeric(14,2)");
        entity.Property(x => x.BalanceAfter).HasColumnName("balance_after").HasColumnType("numeric(14,2)");
        entity.Property(x => x.Reference).HasColumnName("reference").HasMaxLength(100).IsRequired();
        entity.HasIndex(x => x.Reference).IsUnique();
        entity.HasIndex(x => x.IssuanceRequestId);
        entity.HasOne<Policy>().WithMany().HasForeignKey(x => x.PolicyId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<PolicyIssuanceRequest>().WithMany().HasForeignKey(x => x.IssuanceRequestId).OnDelete(DeleteBehavior.Restrict);
    }
}
