using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> entity)
    {
        entity.ToTable("refresh_tokens");
        entity.HasKey(x => x.Id);
        entity.Ignore(x => x.IsActive);
        entity.Property(x => x.UserId).HasColumnName("user_id");
        entity.Property(x => x.TokenHash).HasColumnName("token_hash").HasMaxLength(256).IsRequired();
        entity.HasIndex(x => x.TokenHash).IsUnique();
        entity.Property(x => x.ExpiresAt).HasColumnName("expires_at");
        entity.Property(x => x.RevokedAt).HasColumnName("revoked_at");
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.Property(x => x.Device).HasColumnName("device").HasMaxLength(150);
        entity.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(80);
        entity.Property(x => x.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
        entity.HasOne(x => x.User).WithMany(x => x.RefreshTokens).HasForeignKey(x => x.UserId);
    }
}
