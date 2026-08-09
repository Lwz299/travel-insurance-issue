using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("users");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        entity.HasIndex(x => x.Email).IsUnique();
        entity.Property(x => x.CompanyId).HasColumnName("company_id");
        entity.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(50);
        entity.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
        entity.Property(x => x.EmailVerified).HasColumnName("email_verified").HasDefaultValue(false);
        entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        entity.Property(x => x.FailedLoginAttempts).HasColumnName("failed_login_attempts").HasDefaultValue(0);
        entity.Property(x => x.LockoutUntil).HasColumnName("lockout_until");
        entity.Property(x => x.LastLoginAt).HasColumnName("last_login_at");
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        entity.HasOne(x => x.Company)
            .WithMany(c => c.Users)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);
        entity.HasIndex(x => x.CompanyId);
    }
}
