using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class PlanCoverageZoneConfiguration : IEntityTypeConfiguration<PlanCoverageZone>
{
    public void Configure(EntityTypeBuilder<PlanCoverageZone> entity)
    {
        entity.ToTable("plan_coverage_zones");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.PlanId).HasColumnName("plan_id").IsRequired();
        entity.Property(x => x.CoverageZoneId).HasColumnName("coverage_zone_id").IsRequired();
        entity.HasOne(x => x.CoverageZone).WithMany(z => z.PlanCoverageZones).HasForeignKey(x => x.CoverageZoneId).OnDelete(DeleteBehavior.Restrict);
        entity.HasIndex(x => new { x.PlanId, x.CoverageZoneId }).IsUnique();
    }
}
