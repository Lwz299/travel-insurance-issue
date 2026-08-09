using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class CoverageZoneConfiguration : IEntityTypeConfiguration<CoverageZone>
{
    public void Configure(EntityTypeBuilder<CoverageZone> entity)
    {
        entity.ToTable("coverage_zones");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.IsWorldwide).HasColumnName("is_worldwide").HasDefaultValue(false);
        entity.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        entity.HasIndex(x => x.Code).IsUnique();
    }
}
