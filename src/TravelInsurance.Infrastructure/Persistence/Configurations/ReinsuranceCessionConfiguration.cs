using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class ReinsuranceCessionConfiguration : IEntityTypeConfiguration<ReinsuranceCession>
{
    public void Configure(EntityTypeBuilder<ReinsuranceCession> entity)
    {
        entity.ToTable("reinsurance_cessions");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.PolicyId).HasColumnName("policy_id").IsRequired();
        entity.Property(x => x.ReinsuranceAgreementId).HasColumnName("reinsurance_agreement_id").IsRequired();
        entity.Property(x => x.ReinsurerCompanyId).HasColumnName("reinsurer_company_id").IsRequired();
        entity.Property(x => x.BordereauBatchId).HasColumnName("bordereau_batch_id");
        entity.Property(x => x.AgreementNumberSnapshot).HasColumnName("agreement_number_snapshot").HasMaxLength(100).IsRequired();
        entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(x => x.GrossPremium).HasColumnName("gross_premium").HasColumnType("numeric(14,2)");
        entity.Property(x => x.CededPremium).HasColumnName("ceded_premium").HasColumnType("numeric(14,2)");
        entity.Property(x => x.GrossSumInsured).HasColumnName("gross_sum_insured").HasColumnType("numeric(14,2)");
        entity.Property(x => x.CededSumInsured).HasColumnName("ceded_sum_insured").HasColumnType("numeric(14,2)");
        entity.Property(x => x.CededSharePercentage).HasColumnName("ceded_share_percentage").HasColumnType("numeric(5,2)");
        entity.Property(x => x.CompanyRetentionPercentage).HasColumnName("company_retention_percentage").HasColumnType("numeric(5,2)");
        entity.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.HasIndex(x => x.PolicyId).IsUnique();
        entity.HasIndex(x => new { x.ReinsuranceAgreementId, x.Status });
        entity.HasOne(x => x.ReinsuranceAgreement).WithMany(a => a.Cessions).HasForeignKey(x => x.ReinsuranceAgreementId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.ReinsurerCompany).WithMany().HasForeignKey(x => x.ReinsurerCompanyId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.BordereauBatch).WithMany(b => b.Cessions).HasForeignKey(x => x.BordereauBatchId).OnDelete(DeleteBehavior.Restrict);
    }
}
