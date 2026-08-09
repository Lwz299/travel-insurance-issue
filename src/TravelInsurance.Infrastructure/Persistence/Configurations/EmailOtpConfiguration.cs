using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class EmailOtpConfiguration : IEntityTypeConfiguration<EmailOtp>
{
    public void Configure(EntityTypeBuilder<EmailOtp> entity)
    {
        entity.ToTable("email_otps");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.UserId).HasColumnName("user_id");
        entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        entity.Property(x => x.OtpHash).HasColumnName("otp_hash").HasMaxLength(256).IsRequired();
        entity.Property(x => x.Purpose).HasColumnName("purpose").HasConversion<string>().HasMaxLength(30);
        entity.Property(x => x.ExpiresAt).HasColumnName("expires_at");
        entity.Property(x => x.Attempts).HasColumnName("attempts").HasDefaultValue(0);
        entity.Property(x => x.IsUsed).HasColumnName("is_used").HasDefaultValue(false);
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.HasIndex(x => new { x.Email, x.Purpose, x.IsUsed });
        entity.HasOne(x => x.User).WithMany(x => x.EmailOtps).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
    }
}
