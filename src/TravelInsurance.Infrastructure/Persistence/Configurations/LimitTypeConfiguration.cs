using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class LimitTypeConfiguration : IEntityTypeConfiguration<LimitType>
{
    public void Configure(EntityTypeBuilder<LimitType> entity)
    {
        entity.ToTable("limit_types");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        entity.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        entity.HasIndex(x => x.Code).IsUnique();

        entity.HasData(
            new LimitType { Id = 1, Code = "AMOUNT", Name = "Amount" },
            new LimitType { Id = 2, Code = "PERCENTAGE", Name = "Percentage" },
            new LimitType { Id = 3, Code = "UNLIMITED", Name = "Unlimited" },
            new LimitType { Id = 4, Code = "PER_PERSON", Name = "Per Person" },
            new LimitType { Id = 5, Code = "PER_TRIP", Name = "Per Trip" });
    }
}
