using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class BenefitTypeConfiguration : IEntityTypeConfiguration<BenefitType>
{
    public void Configure(EntityTypeBuilder<BenefitType> entity)
    {
        entity.ToTable("benefit_types");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        entity.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        entity.HasIndex(x => x.Code).IsUnique();

        entity.HasData(
            new BenefitType { Id = 1, Code = "FINANCIAL", Name = "Financial" },
            new BenefitType { Id = 2, Code = "SERVICE", Name = "Service" },
            new BenefitType { Id = 3, Code = "ASSISTANCE", Name = "Assistance" },
            new BenefitType { Id = 4, Code = "REIMBURSEMENT", Name = "Reimbursement" });
    }
}
