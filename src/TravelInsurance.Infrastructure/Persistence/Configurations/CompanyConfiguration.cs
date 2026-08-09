using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> entity)
    {
        entity.ToTable("companies");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.LogoUrl).HasColumnName("logo_url").HasMaxLength(500);
        entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(255);
        entity.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(50);
        entity.Property(x => x.Website).HasColumnName("website").HasMaxLength(255);
        entity.Property(x => x.Address).HasColumnName("address").HasMaxLength(500);
        entity.Property(x => x.TaxNumber).HasColumnName("tax_number").HasMaxLength(100);
        entity.Property(x => x.RegistrationNumber).HasColumnName("registration_number").HasMaxLength(100);
        entity.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        entity.HasIndex(x => x.Type);
        entity.HasIndex(x => x.IsActive);
    }
}
