using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class CoverageBenefitConfiguration : IEntityTypeConfiguration<CoverageBenefit>
{
    public void Configure(EntityTypeBuilder<CoverageBenefit> entity)
    {
        entity.ToTable("coverage_benefits");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.CoverageId).HasColumnName("coverage_id").IsRequired();
        entity.Property(x => x.BenefitId).HasColumnName("benefit_id").IsRequired();
        entity.Property(x => x.LimitTypeId).HasColumnName("limit_type_id").IsRequired();
        entity.Property(x => x.LimitValue).HasColumnName("limit_value").HasColumnType("numeric(14,2)");
        entity.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
        entity.Property(x => x.Deductible).HasColumnName("deductible").HasColumnType("numeric(14,2)");
        entity.Property(x => x.WaitingPeriodDays).HasColumnName("waiting_period_days");
        entity.Property(x => x.DisplayOrder).HasColumnName("display_order").HasDefaultValue(1);
        entity.Property(x => x.IsOptional).HasColumnName("is_optional").HasDefaultValue(false);
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");

        entity.HasOne(x => x.Coverage).WithMany(c => c.CoverageBenefits).HasForeignKey(x => x.CoverageId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(x => x.Benefit).WithMany(b => b.CoverageBenefits).HasForeignKey(x => x.BenefitId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.LimitType).WithMany(l => l.CoverageBenefits).HasForeignKey(x => x.LimitTypeId).OnDelete(DeleteBehavior.Restrict);
        entity.HasIndex(x => new { x.CoverageId, x.BenefitId }).IsUnique();
    }
}
