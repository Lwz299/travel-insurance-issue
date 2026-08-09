using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class PlanCoverageAssignmentConfiguration : IEntityTypeConfiguration<PlanCoverageAssignment>
{
    public void Configure(EntityTypeBuilder<PlanCoverageAssignment> entity)
    {
        entity.ToTable("plan_coverages");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.PlanId).HasColumnName("plan_id").IsRequired();
        entity.Property(x => x.CoverageId).HasColumnName("coverage_id").IsRequired();
        entity.Property(x => x.CustomLimit).HasColumnName("custom_limit").HasColumnType("numeric(14,2)");
        entity.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
        entity.Property(x => x.CustomDeductible).HasColumnName("custom_deductible").HasColumnType("numeric(14,2)");
        entity.Property(x => x.IsEnabled).HasColumnName("is_enabled").HasDefaultValue(true);
        entity.Property(x => x.DisplayOrder).HasColumnName("display_order").HasDefaultValue(1);
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");

        entity.HasOne(x => x.Plan).WithMany(p => p.CoverageAssignments).HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(x => x.Coverage).WithMany(c => c.PlanAssignments).HasForeignKey(x => x.CoverageId).OnDelete(DeleteBehavior.Restrict);
        entity.HasIndex(x => new { x.PlanId, x.CoverageId }).IsUnique();
    }
}
