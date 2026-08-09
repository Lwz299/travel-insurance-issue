using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> entity)
    {
        entity.ToTable("role_permissions");
        entity.HasKey(x => new { x.RoleId, x.PermissionId });
        entity.Property(x => x.RoleId).HasColumnName("role_id");
        entity.Property(x => x.PermissionId).HasColumnName("permission_id");
        entity.HasOne(x => x.Role).WithMany(x => x.RolePermissions).HasForeignKey(x => x.RoleId);
        entity.HasOne(x => x.Permission).WithMany(x => x.RolePermissions).HasForeignKey(x => x.PermissionId);

        entity.HasData(
            // SUPER_ADMIN (5): all permissions
            new RolePermission { RoleId = 5, PermissionId = 1 },
            new RolePermission { RoleId = 5, PermissionId = 2 },
            new RolePermission { RoleId = 5, PermissionId = 3 },
            new RolePermission { RoleId = 5, PermissionId = 4 },
            new RolePermission { RoleId = 5, PermissionId = 5 },
            new RolePermission { RoleId = 5, PermissionId = 6 },
            new RolePermission { RoleId = 5, PermissionId = 7 },
            new RolePermission { RoleId = 5, PermissionId = 8 },
            new RolePermission { RoleId = 5, PermissionId = 9 },
            new RolePermission { RoleId = 5, PermissionId = 10 },
            new RolePermission { RoleId = 5, PermissionId = 11 },
            new RolePermission { RoleId = 5, PermissionId = 12 },
            new RolePermission { RoleId = 5, PermissionId = 13 },
            new RolePermission { RoleId = 5, PermissionId = 14 },
            new RolePermission { RoleId = 5, PermissionId = 15 },
            new RolePermission { RoleId = 5, PermissionId = 16 },
            new RolePermission { RoleId = 5, PermissionId = 17 },
            new RolePermission { RoleId = 5, PermissionId = 18 },
            new RolePermission { RoleId = 5, PermissionId = 19 },
            new RolePermission { RoleId = 5, PermissionId = 20 },
            new RolePermission { RoleId = 5, PermissionId = 21 },
            new RolePermission { RoleId = 5, PermissionId = 22 },
            new RolePermission { RoleId = 5, PermissionId = 23 },
            // ADMIN (1): company management scope
            new RolePermission { RoleId = 1, PermissionId = 1 },
            new RolePermission { RoleId = 1, PermissionId = 2 },
            new RolePermission { RoleId = 1, PermissionId = 3 },
            new RolePermission { RoleId = 1, PermissionId = 4 },
            new RolePermission { RoleId = 1, PermissionId = 5 },
            new RolePermission { RoleId = 1, PermissionId = 6 },
            new RolePermission { RoleId = 1, PermissionId = 8 },
            new RolePermission { RoleId = 1, PermissionId = 9 },
            new RolePermission { RoleId = 1, PermissionId = 10 },
            new RolePermission { RoleId = 1, PermissionId = 12 },
            new RolePermission { RoleId = 1, PermissionId = 13 },
            new RolePermission { RoleId = 1, PermissionId = 14 },
            new RolePermission { RoleId = 1, PermissionId = 15 },
            new RolePermission { RoleId = 1, PermissionId = 16 },
            new RolePermission { RoleId = 1, PermissionId = 17 },
            new RolePermission { RoleId = 1, PermissionId = 18 },
            new RolePermission { RoleId = 1, PermissionId = 19 },
            new RolePermission { RoleId = 1, PermissionId = 20 },
            new RolePermission { RoleId = 1, PermissionId = 21 },
            new RolePermission { RoleId = 1, PermissionId = 22 },
            new RolePermission { RoleId = 1, PermissionId = 23 },
            // AGENCY_USER (3)
            new RolePermission { RoleId = 3, PermissionId = 1 },
            new RolePermission { RoleId = 3, PermissionId = 2 },
            new RolePermission { RoleId = 3, PermissionId = 6 },
            new RolePermission { RoleId = 3, PermissionId = 12 },
            new RolePermission { RoleId = 3, PermissionId = 13 },
            new RolePermission { RoleId = 3, PermissionId = 14 },
            new RolePermission { RoleId = 3, PermissionId = 15 },
            new RolePermission { RoleId = 3, PermissionId = 16 },
            new RolePermission { RoleId = 3, PermissionId = 17 },
            new RolePermission { RoleId = 3, PermissionId = 18 },
            // CUSTOMER (2)
            new RolePermission { RoleId = 2, PermissionId = 1 }
        );
    }
}

