using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class PolicyCoverageSnapshotConfiguration : IEntityTypeConfiguration<PolicyCoverageSnapshot>
{
    public void Configure(EntityTypeBuilder<PolicyCoverageSnapshot> entity)
    {
        entity.ToTable("policy_coverage_snapshots");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.PolicyId).HasColumnName("policy_id");
        entity.Property(x => x.CoverageId).HasColumnName("coverage_id");
        entity.Property(x => x.CoverageCode).HasColumnName("coverage_code").HasMaxLength(100).IsRequired();
        entity.Property(x => x.CoverageName).HasColumnName("coverage_name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.Limit).HasColumnName("limit_value").HasColumnType("numeric(14,2)");
        entity.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
        entity.Property(x => x.Deductible).HasColumnName("deductible").HasColumnType("numeric(14,2)");
        entity.HasMany(x => x.BenefitSnapshots).WithOne(x => x.PolicyCoverageSnapshot!).HasForeignKey(x => x.PolicyCoverageSnapshotId).OnDelete(DeleteBehavior.Cascade);
    }
}
