using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class PolicyFundingConfiguration : IEntityTypeConfiguration<PolicyFunding>
{
    public void Configure(EntityTypeBuilder<PolicyFunding> entity)
    {
        entity.ToTable("policy_fundings");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.IssuanceRequestId).HasColumnName("issuance_request_id");
        entity.Property(x => x.PolicyId).HasColumnName("policy_id");
        entity.Property(x => x.AgencyCompanyId).HasColumnName("agency_company_id");
        entity.Property(x => x.PaymentId).HasColumnName("payment_id");
        entity.Property(x => x.FundingMode).HasColumnName("funding_mode").HasConversion<string>().HasMaxLength(30);
        entity.Property(x => x.Amount).HasColumnName("amount").HasColumnType("numeric(14,2)");
        entity.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        entity.Property(x => x.FundingReference).HasColumnName("funding_reference").HasMaxLength(100).IsRequired();
        entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
        entity.HasIndex(x => x.IssuanceRequestId).IsUnique();
        entity.HasIndex(x => x.PaymentId).IsUnique();
        entity.HasIndex(x => x.FundingReference).IsUnique();
        entity.HasOne<PolicyIssuanceRequest>().WithMany().HasForeignKey(x => x.IssuanceRequestId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<Policy>().WithMany().HasForeignKey(x => x.PolicyId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<Company>().WithMany().HasForeignKey(x => x.AgencyCompanyId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<Payment>().WithMany().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Restrict);
    }
}
