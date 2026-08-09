using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class BenefitConfiguration : IEntityTypeConfiguration<Benefit>
{
    public void Configure(EntityTypeBuilder<Benefit> entity)
    {
        entity.ToTable("benefits");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.BenefitTypeId).HasColumnName("benefit_type_id").IsRequired();
        entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.Description).HasColumnName("description");
        entity.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        entity.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        entity.HasOne(x => x.BenefitType).WithMany(t => t.Benefits).HasForeignKey(x => x.BenefitTypeId).OnDelete(DeleteBehavior.Restrict);
        entity.HasIndex(x => x.Code).IsUnique();
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
