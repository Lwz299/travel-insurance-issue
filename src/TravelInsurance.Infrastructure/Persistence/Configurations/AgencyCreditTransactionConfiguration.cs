using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class AgencyCreditTransactionConfiguration : IEntityTypeConfiguration<AgencyCreditTransaction>
{
    public void Configure(EntityTypeBuilder<AgencyCreditTransaction> entity)
    {
        entity.ToTable("agency_credit_transactions");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.CreditAccountId).HasColumnName("credit_account_id");
        entity.Property(x => x.PolicyId).HasColumnName("policy_id");
        entity.Property(x => x.IssuanceRequestId).HasColumnName("issuance_request_id");
        entity.Property(x => x.SettlementId).HasColumnName("settlement_id");
        entity.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(30);
        entity.Property(x => x.Amount).HasColumnName("amount").HasColumnType("numeric(14,2)");
        entity.Property(x => x.OutstandingBefore).HasColumnName("outstanding_before").HasColumnType("numeric(14,2)");
        entity.Property(x => x.OutstandingAfter).HasColumnName("outstanding_after").HasColumnType("numeric(14,2)");
        entity.Property(x => x.Reference).HasColumnName("reference").HasMaxLength(100).IsRequired();
        entity.HasIndex(x => x.Reference).IsUnique();
        entity.HasIndex(x => x.IssuanceRequestId);
        entity.HasOne<Policy>().WithMany().HasForeignKey(x => x.PolicyId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<PolicyIssuanceRequest>().WithMany().HasForeignKey(x => x.IssuanceRequestId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<AgencySettlement>().WithMany().HasForeignKey(x => x.SettlementId).OnDelete(DeleteBehavior.Restrict);
    }
}
