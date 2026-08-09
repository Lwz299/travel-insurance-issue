using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class PlanCoverageBenefitOverrideConfiguration : IEntityTypeConfiguration<PlanCoverageBenefitOverride>
{
    public void Configure(EntityTypeBuilder<PlanCoverageBenefitOverride> entity)
    {
        entity.ToTable("plan_coverage_benefit_overrides");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.PlanCoverageId).HasColumnName("plan_coverage_id").IsRequired();
        entity.Property(x => x.BenefitId).HasColumnName("benefit_id").IsRequired();
        entity.Property(x => x.LimitTypeId).HasColumnName("limit_type_id").IsRequired();
        entity.Property(x => x.LimitOverride).HasColumnName("limit_override").HasColumnType("numeric(14,2)");
        entity.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
        entity.Property(x => x.DeductibleOverride).HasColumnName("deductible_override").HasColumnType("numeric(14,2)");
        entity.Property(x => x.IsEnabled).HasColumnName("is_enabled").HasDefaultValue(true);

        entity.HasOne(x => x.PlanCoverage).WithMany(p => p.BenefitOverrides).HasForeignKey(x => x.PlanCoverageId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(x => x.Benefit).WithMany().HasForeignKey(x => x.BenefitId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.LimitType).WithMany(l => l.PlanBenefitOverrides).HasForeignKey(x => x.LimitTypeId).OnDelete(DeleteBehavior.Restrict);
        entity.HasIndex(x => new { x.PlanCoverageId, x.BenefitId }).IsUnique();
    }
}
