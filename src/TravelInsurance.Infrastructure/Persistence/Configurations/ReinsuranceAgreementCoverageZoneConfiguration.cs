using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class ReinsuranceAgreementCoverageZoneConfiguration : IEntityTypeConfiguration<ReinsuranceAgreementCoverageZone>
{
    public void Configure(EntityTypeBuilder<ReinsuranceAgreementCoverageZone> entity)
    {
        entity.ToTable("reinsurance_agreement_coverage_zones");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ReinsuranceAgreementId).HasColumnName("reinsurance_agreement_id").IsRequired();
        entity.Property(x => x.CoverageZoneId).HasColumnName("coverage_zone_id").IsRequired();
        entity.HasOne(x => x.ReinsuranceAgreement).WithMany(a => a.CoverageZones).HasForeignKey(x => x.ReinsuranceAgreementId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(x => x.CoverageZone).WithMany(z => z.ReinsuranceAgreementCoverageZones).HasForeignKey(x => x.CoverageZoneId).OnDelete(DeleteBehavior.Restrict);
        entity.HasIndex(x => new { x.ReinsuranceAgreementId, x.CoverageZoneId }).IsUnique();
    }
}
