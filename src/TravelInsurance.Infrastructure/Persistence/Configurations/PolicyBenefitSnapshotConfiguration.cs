using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class PolicyBenefitSnapshotConfiguration : IEntityTypeConfiguration<PolicyBenefitSnapshot>
{
    public void Configure(EntityTypeBuilder<PolicyBenefitSnapshot> entity)
    {
        entity.ToTable("policy_benefit_snapshots");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.PolicyCoverageSnapshotId).HasColumnName("policy_coverage_snapshot_id");
        entity.Property(x => x.BenefitId).HasColumnName("benefit_id");
        entity.Property(x => x.BenefitCode).HasColumnName("benefit_code").HasMaxLength(100).IsRequired();
        entity.Property(x => x.BenefitName).HasColumnName("benefit_name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.LimitTypeId).HasColumnName("limit_type_id");
        entity.Property(x => x.Limit).HasColumnName("limit_value").HasColumnType("numeric(14,2)");
        entity.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
        entity.Property(x => x.Deductible).HasColumnName("deductible").HasColumnType("numeric(14,2)");
        entity.Property(x => x.WaitingPeriodDays).HasColumnName("waiting_period_days");
        entity.HasIndex(x => x.PolicyCoverageSnapshotId);
    }
}
