using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class CoverageGroupConfiguration : IEntityTypeConfiguration<CoverageGroup>
{
    public void Configure(EntityTypeBuilder<CoverageGroup> entity)
    {
        entity.ToTable("coverage_groups");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.SortOrder).HasColumnName("sort_order");
        entity.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        entity.HasIndex(x => x.Code).IsUnique();
    }
}
