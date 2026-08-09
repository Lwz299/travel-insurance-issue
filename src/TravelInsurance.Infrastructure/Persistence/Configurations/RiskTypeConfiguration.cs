using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class RiskTypeConfiguration : IEntityTypeConfiguration<RiskType>
{
    public void Configure(EntityTypeBuilder<RiskType> entity)
    {
        entity.ToTable("risk_types");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        entity.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        entity.HasIndex(x => x.Code).IsUnique();

        entity.HasData(
            new RiskType { Id = 1, Code = "MEDICAL", Name = "Medical" },
            new RiskType { Id = 2, Code = "TRAVEL", Name = "Travel" },
            new RiskType { Id = 3, Code = "PROPERTY", Name = "Property" },
            new RiskType { Id = 4, Code = "LIABILITY", Name = "Liability" },
            new RiskType { Id = 5, Code = "LIFE", Name = "Life" },
            new RiskType { Id = 6, Code = "ACCIDENT", Name = "Accident" });
    }
}
