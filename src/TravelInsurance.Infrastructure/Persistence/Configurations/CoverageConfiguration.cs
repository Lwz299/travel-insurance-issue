using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class CoverageConfiguration : IEntityTypeConfiguration<Coverage>
{
    public void Configure(EntityTypeBuilder<Coverage> entity)
    {
        entity.ToTable("coverages");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.GroupId).HasColumnName("group_id").IsRequired();
        entity.Property(x => x.RiskTypeId).HasColumnName("risk_type_id").IsRequired();
        entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.Description).HasColumnName("description");
        entity.Property(x => x.IsMandatory).HasColumnName("is_mandatory").HasDefaultValue(false);
        entity.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        entity.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        entity.HasOne(x => x.Group).WithMany(g => g.Coverages).HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.RiskType).WithMany(r => r.Coverages).HasForeignKey(x => x.RiskTypeId).OnDelete(DeleteBehavior.Restrict);
        entity.HasIndex(x => x.Code).IsUnique();
        entity.HasIndex(x => x.GroupId);
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
