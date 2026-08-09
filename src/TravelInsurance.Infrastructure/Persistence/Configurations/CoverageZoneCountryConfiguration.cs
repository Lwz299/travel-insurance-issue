using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class CoverageZoneCountryConfiguration : IEntityTypeConfiguration<CoverageZoneCountry>
{
    public void Configure(EntityTypeBuilder<CoverageZoneCountry> entity)
    {
        entity.ToTable("coverage_zone_countries");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.CoverageZoneId).HasColumnName("coverage_zone_id").IsRequired();
        entity.Property(x => x.CountryId).HasColumnName("country_id").IsRequired();
        entity.Property(x => x.IsExcluded).HasColumnName("is_excluded").HasDefaultValue(false);
        entity.HasOne(x => x.CoverageZone).WithMany(z => z.Countries).HasForeignKey(x => x.CoverageZoneId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(x => x.Country).WithMany(c => c.CoverageZoneCountries).HasForeignKey(x => x.CountryId).OnDelete(DeleteBehavior.Restrict);
        entity.HasIndex(x => new { x.CoverageZoneId, x.CountryId }).IsUnique();
    }
}
