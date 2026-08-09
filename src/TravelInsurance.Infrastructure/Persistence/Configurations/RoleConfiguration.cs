using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> entity)
    {
        entity.ToTable("roles");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
        entity.HasIndex(x => x.Name).IsUnique();
        entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);

        entity.HasData(
            new Role { Id = 1, Name = "ADMIN", Description = "Insurance company administrator" },
            new Role { Id = 2, Name = "CUSTOMER", Description = "Direct customer" },
            new Role { Id = 3, Name = "AGENCY_USER", Description = "Agency user" },
            new Role { Id = 4, Name = "REINSURER_USER", Description = "Reinsurer user" },
            new Role { Id = 5, Name = "SUPER_ADMIN", Description = "System-level super administrator" }
        );
    }
}
